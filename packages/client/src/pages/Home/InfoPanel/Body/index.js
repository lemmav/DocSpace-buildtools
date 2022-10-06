import React, { useState, useEffect } from "react";
import { withRouter } from "react-router";
import { inject, observer } from "mobx-react";

import ViewHelper from "./helpers/ViewHelper";
import ItemTitle from "./sub-components/ItemTitle";

import { StyledInfoPanelBody } from "./styles/common";
import { getRoomInfo } from "@docspace/common/api/rooms";

const InfoPanelBodyContent = ({
  t,
  selection,
  setSelection,
  updateSelection,
  setUpdateSelection,
  normalizeSelection,
  selectionParentRoom,
  setSelectionParentRoom,
  roomsView,
  fileView,
  getIsFiles,
  getIsRooms,
  getIsAccounts,
  getIsGallery,
  gallerySelected,
  setBufferSelection,
  isRootFolder,
  getIcon,
  getFolderIcon,
  ...props
}) => {
  const [selectedItems, setSelectedItems] = useState(props.selectedItems);
  const [selectedFolder, setSelectedFolder] = useState(props.selectedFolder);

  const isFiles = getIsFiles();
  const isRooms = getIsRooms();
  const isAccounts = getIsAccounts();
  const isGallery = getIsGallery();
  const isNoItem =
    (isGallery && !gallerySelected) ||
    ((isRootFolder || isAccounts) && props.selectedItems.length === 0);
  const isSeveralItems = props.selectedItems.length > 1;

  const viewHelper = new ViewHelper({
    defaultProps: {
      selection,
      isFiles,
      isRooms,
      isAccounts,
      isGallery,
      isRootFolder,
      personal: props.personal,
      culture: props.culture,
    },
    detailsProps: {
      setSelection,
      getItemIcon: props.getItemIcon,
      getFolderInfo: props.getFolderInfo,
      onSelectItem: props.onSelectItem,
      createThumbnail: props.createThumbnail,
    },
    membersProps: {
      selectionParentRoom,
      setSelectionParentRoom,
      selfId: props.selfId,
      isOwner: props.isOwner,
      isAdmin: props.isAdmin,
      getRoomMembers: props.getRoomMembers,
      changeUserType: props.changeUserType,
    },
    historyProps: {
      setSelection,
      getHistory: props.getHistory,
      getRoomHistory: props.getRoomHistory,
      getFileHistory: props.getFileHistory,
      getItemIcon: props.getItemIcon,
      openLocationAction: props.openLocationAction,
    },
    accountsProps: {
      selfId: props.selfId,
      isOwner: props.isOwner,
      isAdmin: props.isAdmin,
      changeUserType: props.changeUserType,
    },
    galleryProps: {
      getIcon,
      gallerySelected,
    },
  });

  const getSelection = () => {
    return selectedItems.length === 0
      ? normalizeSelection({ ...selectedFolder, isSelectedFolder: true })
      : selectedItems.length === 1
      ? normalizeSelection({ ...selectedItems[0], isSelectedItem: true })
      : [...Array(props.selectedItems.length).keys()];
  };

  const getView = () => {
    if (isNoItem) return viewHelper.NoItemView();
    if (isSeveralItems) return viewHelper.SeveralItemsView();
    if (isGallery) return viewHelper.GalleryView();
    if (isAccounts) return viewHelper.AccountsView();

    switch (isRooms ? roomsView : fileView) {
      case "members": {
        return viewHelper.MembersView();
      }
      case "history": {
        return viewHelper.HistoryView();
      }
      case "details": {
        return viewHelper.DetailsView();
      }
    }
  };

  //////////////////////////////////////////////////////////

  // Removing mark set with choosing item from context menu
  // for it to updated after selctedItems or selectedFolder change
  // (only for non-oforms items)
  useEffect(() => {
    if (isGallery || !selection?.isContextMenuSelection) return;
    setSelection(normalizeSelection(selection));
  }, [selection]);

  // Setting selection after selectedItems or selectedFolder update
  useEffect(() => {
    if (selection && selection.isContextMenuSelection) return;
    const newSelection = getSelection();
    if (selection && selection?.id === newSelection.id && !newSelection.length)
      return;
    setSelection(newSelection);
  }, [selectedItems, selectedFolder]);

  //////////////////////////////////////////////////////////

  // Updating SelectedItems only if
  // a) Length of an array changed
  // b) Single chosen item changed
  useEffect(() => {
    const selectedItemsLengthChanged =
      selectedItems.length !== props.selectedItems.length;
    const singleSelectedItemChanged =
      selectedItems[0]?.id !== props.selectedItems[0]?.id;
    if (selectedItemsLengthChanged || singleSelectedItemChanged)
      setSelectedItems(props.selectedItems);
  }, [props.selectedItems]);

  // Updating SelectedFolder only if
  //   a) Selected folder changed
  useEffect(() => {
    if (selectedFolder.id !== props.selectedFolder.id)
      setSelectedFolder(props.selectedFolder);
  }, [props.selectedFolder]);

  // Updating selectionParentRoom after selectFolder change
  // if it is located in another room
  useEffect(async () => {
    if (isGallery || isAccounts) return;

    const currentFolderRoomId = selectedFolder.pathParts[1];
    const storeRoomId = selectionParentRoom?.id;

    if (!currentFolderRoomId) return;
    if (currentFolderRoomId === storeRoomId) return;

    const newSelectionParentRoom = await getRoomInfo(currentFolderRoomId);
    if (storeRoomId !== newSelectionParentRoom.id)
      setSelectionParentRoom(normalizeSelection(newSelectionParentRoom));
  }, [selectedFolder]);

  //////////////////////////////////////////////////////////

  // useEffect(() => {
  //   console.log("\nfor-dev  Selected items: ", selectedItems);
  //   console.log("\nfor-dev  Selected folder: ", selectedFolder);
  // }, [selectedItems, selectedFolder]);

  //////////////////////////////////////////////////////////

  if (!selection && !isGallery) return null;

  // console.log(props.selectedItems);
  return (
    <StyledInfoPanelBody>
      {!isNoItem && (
        <ItemTitle
          selection={selection}
          selectionParentRoom={selectionParentRoom}
          roomsView={roomsView}
          isRooms={isRooms}
          isAccounts={isAccounts}
          isSeveralItems={isSeveralItems}
          isGallery={isGallery}
          getIcon={getIcon}
          setBufferSelection={setBufferSelection}
          getContextOptions={props.getContextOptions}
          getContextOptionActions={props.getContextOptionActions}
          getUserContextOptions={props.getUserContextOptions}
        />
      )}
      {getView()}
    </StyledInfoPanelBody>
  );
};

export default inject(
  ({
    auth,
    filesStore,
    settingsStore,
    filesActionsStore,
    dialogsStore,
    selectedFolderStore,
    oformsStore,
    contextOptionsStore,
    peopleStore,
  }) => {
    const { isOwner, isAdmin, id: selfId } = auth.userStore.user;
    const { personal, culture } = auth.settingsStore;
    const { getIcon, getFolderIcon } = settingsStore;
    const { onSelectItem, openLocationAction } = filesActionsStore;
    const { changeType: changeUserType } = peopleStore;
    const { setSharingPanelVisible } = dialogsStore;
    const { isRootFolder } = selectedFolderStore;
    const { gallerySelected } = oformsStore;
    const {
      getFilesContextOptions: getContextOptionActions,
    } = contextOptionsStore;

    const {
      selection,
      setSelection,
      updateSelection,
      setUpdateSelection,
      selectionParentRoom,
      setSelectionParentRoom,
      normalizeSelection,
      roomsView,
      fileView,
      getItemIcon,
      getIsFiles,
      getIsRooms,
      getIsAccounts,
      getIsGallery,
    } = auth.infoPanelStore;

    const {
      selection: filesStoreSelection,
      getFilesContextOptions: getContextOptions,
      setBufferSelection,
      getFolderInfo,
      getShareUsers,
      getRoomMembers,
      getHistory,
      getRoomHistory,
      getFileHistory,
      createThumbnail,
    } = filesStore;

    const {
      selection: peopleStoreSelection,
      bufferSelection: peopleStoreBufferSelection,
    } = peopleStore.selectionStore;
    const { getUserContextOptions } = peopleStore.contextOptionsStore;

    const selectedFiles =
      filesStoreSelection?.length > 0 ? [...filesStoreSelection] : [];
    const selectedUsers = peopleStoreSelection.length
      ? [...peopleStoreSelection]
      : peopleStoreBufferSelection
      ? [peopleStoreBufferSelection]
      : [];

    const selectedItems = getIsAccounts() ? selectedUsers : selectedFiles;
    const selectedFolder = {
      ...selectedFolderStore,
      isFolder: true,
      isRoom: !!selectedFolderStore.roomType,
    };

    return {
      selfId,
      isOwner,
      isAdmin,
      personal,
      culture,

      selection,
      setSelection,
      updateSelection,
      setUpdateSelection,
      selectionParentRoom,
      setSelectionParentRoom,
      normalizeSelection,
      roomsView,
      fileView,
      getItemIcon,
      getIsFiles,
      getIsRooms,
      getIsAccounts,
      getIsGallery,

      selectedItems,
      selectedFolder,
      setBufferSelection,

      getContextOptions,
      getContextOptionActions,
      getUserContextOptions,

      getFolderInfo,
      onSelectItem,
      getShareUsers,
      getRoomMembers,
      changeUserType,
      getHistory,
      getRoomHistory,
      getFileHistory,
      setSharingPanelVisible,

      getIcon,
      getFolderIcon,
      createThumbnail,
      openLocationAction,

      gallerySelected,

      isRootFolder,
    };
  }
)(withRouter(observer(InfoPanelBodyContent)));
