import React, { useEffect } from "react";
import { observer, inject } from "mobx-react";
import { useNavigate, useLocation } from "react-router-dom";
import Section from "@docspace/common/components/Section";
import Loader from "@docspace/components/loader";
import { ValidationResult } from "../../helpers/constants";

import RoomPassword from "./sub-components/RoomPassword";
import RoomErrors from "./sub-components/RoomErrors";

import PublicRoomPage from "./PublicRoomPage";

import FilesFilter from "@docspace/common/api/files/filter";

const PublicRoom = (props) => {
  const {
    isLoaded,
    isLoading,
    roomStatus,
    roomId,
    validatePublicRoomKey,
    getFilesSettings,
  } = props;

  const navigate = useNavigate();
  const location = useLocation();
  const lastKeySymbol = location.search.indexOf("&");

  const lastIndex =
    lastKeySymbol === -1 ? location.search.length : lastKeySymbol;
  const key = location.search.substring(5, lastIndex);

  useEffect(() => {
    validatePublicRoomKey(key);
  }, [validatePublicRoomKey]);

  const setPublicRoomFilter = (filterObj) => {
    const newFilter = new FilesFilter();

    newFilter.filterType = filterObj.filterType;
    newFilter.page = filterObj.page;
    newFilter.pageCount = filterObj.pageCount;
    newFilter.search = filterObj.search;
    newFilter.sortBy = filterObj.sortBy;
    newFilter.sortOrder = filterObj.sortOrder;
    newFilter.total = filterObj.total;
    newFilter.viewAs = filterObj.viewAs;
    newFilter.withSubfolders = filterObj.withSubfolders;
    newFilter.folder = filterObj.folder;

    return newFilter;
  };

  const fetchRoomFiles = async () => {
    await getFilesSettings();

    const filterObj = FilesFilter.getFilter(window.location);

    if (filterObj?.folder && filterObj?.folder !== "@my") {
      const filter = setPublicRoomFilter(filterObj);

      const url = `${location.pathname}?key=${key}&${filter.toUrlParams()}`;

      navigate(url);
    } else {
      const newFilter = FilesFilter.getDefault();
      newFilter.folder = roomId;

      const url = `${location.pathname}?key=${key}&${newFilter.toUrlParams()}`;

      navigate(url);
    }
  };

  useEffect(() => {
    if (isLoaded) fetchRoomFiles();
  }, [isLoaded]);

  const renderLoader = () => {
    return (
      <Section>
        <Section.SectionBody>
          <Loader className="pageLoader" type="rombs" size="40px" />
        </Section.SectionBody>
      </Section>
    );
  };

  const renderPage = () => {
    switch (roomStatus) {
      case ValidationResult.Ok:
        return <PublicRoomPage />;
      case ValidationResult.Invalid:
        return <RoomErrors isInvalid />;
      case ValidationResult.Expired:
        return <RoomErrors />;
      case ValidationResult.Password:
        return <RoomPassword roomKey={key} />;

      default:
        return renderLoader();
    }
  };

  return isLoading ? (
    renderLoader()
  ) : isLoaded ? (
    <PublicRoomPage />
  ) : (
    renderPage()
  );
};

export default inject(({ publicRoomStore, settingsStore }) => {
  const { validatePublicRoomKey, isLoaded, isLoading, roomStatus, roomId } =
    publicRoomStore;

  const { getFilesSettings } = settingsStore;

  return {
    roomId,
    isLoaded,
    isLoading,
    roomStatus,

    getFilesSettings,

    validatePublicRoomKey,
  };
})(observer(PublicRoom));
