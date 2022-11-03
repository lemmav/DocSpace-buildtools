import React, { useState, useEffect, useCallback, useMemo } from "react";
import { withTranslation } from "react-i18next";
import { withRouter } from "react-router";
import toastr from "@docspace/components/toast/toastr";
import { inject, observer } from "mobx-react";
import Button from "@docspace/components/button";
import Tooltip from "@docspace/components/tooltip";
import Text from "@docspace/components/text";
import TabContainer from "@docspace/components/tabs-container";
import Preview from "./Appearance/preview";

import ColorSchemeDialog from "./sub-components/colorSchemeDialog";

import DropDownItem from "@docspace/components/drop-down-item";
import DropDownContainer from "@docspace/components/drop-down";

import HexColorPickerComponent from "./sub-components/hexColorPicker";
import { isMobileOnly } from "react-device-detect";

import Loader from "./sub-components/loaderAppearance";

import { StyledComponent } from "./Appearance/StyledApperance.js";

import BreakpointWarning from "../../../../components/BreakpointWarning/index";
import ModalDialogDelete from "./sub-components/modalDialogDelete";

const Appearance = (props) => {
  const {
    appearanceTheme,
    selectedThemeId,
    sendAppearanceTheme,
    getAppearanceTheme,
    currentColorScheme,

    deleteAppearanceTheme,
    tReady,
    t,
  } = props;

  const [previewTheme, setPreviewTheme] = useState("Light theme");

  const [showColorSchemeDialog, setShowColorSchemeDialog] = useState(false);

  const [headerColorSchemeDialog, setHeaderColorSchemeDialog] = useState(
    "Edit color scheme"
  );

  const [currentColorAccent, setCurrentColorAccent] = useState(null);
  const [currentColorButtons, setCurrentColorButtons] = useState(null);

  const [openHexColorPickerAccent, setOpenHexColorPickerAccent] = useState(
    false
  );
  const [openHexColorPickerButtons, setOpenHexColorPickerButtons] = useState(
    false
  );

  //TODO: Add default color
  const [appliedColorAccent, setAppliedColorAccent] = useState("#F97A0B");
  const [appliedColorButtons, setAppliedColorButtons] = useState("#FF9933");

  const [changeCurrentColorAccent, setChangeCurrentColorAccent] = useState(
    false
  );
  const [changeCurrentColorButtons, setChangeCurrentColorButtons] = useState(
    false
  );

  const [viewMobile, setViewMobile] = useState(false);

  const [showSaveButtonDialog, setShowSaveButtonDialog] = useState(false);

  const [isEditDialog, setIsEditDialog] = useState(false);
  const [isAddThemeDialog, setIsAddThemeDialog] = useState(false);

  const [previewAccentColor, setPreviewAccentColor] = useState(
    currentColorScheme.accentColor
  );
  const [selectThemeId, setSelectThemeId] = useState(selectedThemeId);

  const [isDisabledSaveButton, setIsDisabledSaveButton] = useState(true);

  const [abilityAddTheme, setAbilityAddTheme] = useState(true);

  const [isDisabledEditButton, setIsDisabledEditButton] = useState(true);
  const [isDisabledDeleteButton, setIsDisabledDeleteButton] = useState(true);
  const [isShowDeleteButton, setIsShowDeleteButton] = useState(false);

  const [visibleDialog, setVisibleDialog] = useState(false);

  const checkImg = (
    <img className="check-img" src="/static/images/check.white.svg" />
  );

  const array_items = useMemo(
    () => [
      {
        key: "0",
        title: t("Profile:LightTheme"),
        content: (
          <Preview
            previewTheme={previewTheme}
            previewAccentColor={previewAccentColor}
            selectThemeId={selectThemeId}
            themePreview="Light"
          />
        ),
      },
      {
        key: "1",
        title: t("Profile:DarkTheme"),
        content: (
          <Preview
            previewTheme={previewTheme}
            previewAccentColor={previewAccentColor}
            selectThemeId={selectThemeId}
            themePreview="Dark"
          />
        ),
      },
    ],
    [previewAccentColor, previewTheme, selectThemeId, tReady]
  );

  useEffect(() => {
    if (appearanceTheme.length === 10) {
      setAbilityAddTheme(false);
    } else {
      setAbilityAddTheme(true);
    }

    if (appearanceTheme.length === 7) {
      setIsShowDeleteButton(false);
    } else {
      setIsShowDeleteButton(true);
    }
  }, [appearanceTheme.length, setAbilityAddTheme, setIsShowDeleteButton]);

  useEffect(() => {
    onCheckView();
    window.addEventListener("resize", onCheckView);

    return () => window.removeEventListener("resize", onCheckView);
  }, []);

  useEffect(() => {
    const localStorageId = +localStorage.getItem("selectThemeId");

    setSelectThemeId(localStorageId ? localStorageId : selectedThemeId);
  }, [selectedThemeId, appearanceTheme.length, setSelectThemeId]);

  useEffect(() => {
    if (selectThemeId < 8) {
      setIsDisabledEditButton(true);
      setIsDisabledDeleteButton(true);
      return;
    }

    setIsDisabledEditButton(false);
    setIsDisabledDeleteButton(false);
  }, [selectThemeId]);

  useEffect(() => {
    if (selectThemeId === selectedThemeId) {
      setIsDisabledSaveButton(true);
    } else {
      setIsDisabledSaveButton(false);
    }

    if (
      changeCurrentColorAccent &&
      changeCurrentColorButtons &&
      isAddThemeDialog
    ) {
      setShowSaveButtonDialog(true);
    }

    if (
      (changeCurrentColorAccent || changeCurrentColorButtons) &&
      isEditDialog
    ) {
      setShowSaveButtonDialog(true);
    }
  }, [
    selectedThemeId,
    selectThemeId,
    changeCurrentColorAccent,
    changeCurrentColorButtons,
    isAddThemeDialog,
    isEditDialog,
    previewAccentColor,
  ]);

  const onCheckView = () => {
    if (isMobileOnly || window.innerWidth < 600) {
      setViewMobile(true);
    } else {
      setViewMobile(false);
    }
  };

  const onColorSelection = (item) => {
    setPreviewAccentColor(item.accentColor);
    setSelectThemeId(item.id);
    localStorage.setItem("selectThemeId", item.id);
  };

  const onShowCheck = useCallback(
    (colorNumber) => {
      return selectThemeId && selectThemeId === colorNumber && checkImg;
    },
    [selectThemeId, checkImg]
  );

  const onChangePreviewTheme = (e) => {
    setPreviewTheme(e.title);
  };

  const onSave = useCallback(async () => {
    setIsDisabledSaveButton(true);

    if (!selectThemeId) return;

    localStorage.removeItem("selectThemeId");
    try {
      await sendAppearanceTheme({ selected: selectThemeId });
      await getAppearanceTheme();

      toastr.success(t("Settings:SuccessfullySaveSettingsMessage"));
    } catch (error) {
      toastr.error(error);
    }
  }, [
    selectThemeId,
    setIsDisabledSaveButton,
    sendAppearanceTheme,
    getAppearanceTheme,
  ]);

  const onClickEdit = () => {
    appearanceTheme.map((item) => {
      if (item.id === selectThemeId) {
        setCurrentColorAccent(item.accentColor);
        setCurrentColorButtons(item.buttonsMain);
      }
    });

    setIsEditDialog(true);

    setHeaderColorSchemeDialog("Edit color scheme");

    setShowColorSchemeDialog(true);
  };

  const onClickDeleteModal = useCallback(async () => {
    try {
      localStorage.removeItem("selectThemeId");

      await deleteAppearanceTheme(selectThemeId);
      await getAppearanceTheme();

      setPreviewAccentColor(currentColorScheme.accentColor);
      setVisibleDialog(false);

      toastr.success(t("Settings:SuccessfullySaveSettingsMessage"));
    } catch (error) {
      toastr.error(error);
    }
  }, [
    selectThemeId,
    setVisibleDialog,
    deleteAppearanceTheme,
    getAppearanceTheme,
  ]);

  const onCloseColorSchemeDialog = () => {
    setShowColorSchemeDialog(false);

    setOpenHexColorPickerAccent(false);
    setOpenHexColorPickerButtons(false);

    setChangeCurrentColorAccent(false);
    setChangeCurrentColorButtons(false);

    setIsEditDialog(false);

    setShowSaveButtonDialog(false);
  };

  const onAddTheme = () => {
    if (!abilityAddTheme) return;
    setIsAddThemeDialog(true);

    setCurrentColorAccent(null);
    setCurrentColorButtons(null);

    setHeaderColorSchemeDialog("New color scheme");

    setShowColorSchemeDialog(true);
  };

  const onClickColor = (e) => {
    if (e.target.id === "accent") {
      setOpenHexColorPickerAccent(true);
      setOpenHexColorPickerButtons(false);
    } else {
      setOpenHexColorPickerButtons(true);
      setOpenHexColorPickerAccent(false);
    }
  };

  const onCloseHexColorPicker = () => {
    setOpenHexColorPickerAccent(false);
    setOpenHexColorPickerButtons(false);
  };

  const onAppliedColorAccent = useCallback(() => {
    setCurrentColorAccent(appliedColorAccent);

    onCloseHexColorPicker();

    if (appliedColorAccent === currentColorAccent) return;

    setChangeCurrentColorAccent(true);
  }, [appliedColorAccent, currentColorAccent]);

  const onAppliedColorButtons = useCallback(() => {
    setCurrentColorButtons(appliedColorButtons);

    onCloseHexColorPicker();

    if (appliedColorButtons === currentColorButtons) return;

    setChangeCurrentColorButtons(true);
  }, [appliedColorButtons]);

  const onSaveNewThemes = useCallback(
    async (theme) => {
      try {
        await sendAppearanceTheme({ themes: [theme] });
        await getAppearanceTheme();

        toastr.success(t("Settings:SuccessfullySaveSettingsMessage"));
      } catch (error) {
        toastr.error(error);
      }
    },
    [sendAppearanceTheme, getAppearanceTheme]
  );

  const onSaveChangedThemes = useCallback(
    async (editTheme) => {
      try {
        await sendAppearanceTheme({ themes: [editTheme] });
        await getAppearanceTheme();
        setPreviewAccentColor(editTheme.accentColor);

        toastr.success(t("Settings:SuccessfullySaveSettingsMessage"));
      } catch (error) {
        toastr.error(error);
      }
    },
    [sendAppearanceTheme, getAppearanceTheme]
  );

  const onSaveColorSchemeDialog = () => {
    if (isAddThemeDialog) {
      // Saving a new custom theme
      const theme = {
        accentColor: currentColorAccent,
        buttonsMain: currentColorButtons,
        textColor: "#FFFFFF",
      };

      onSaveNewThemes(theme);

      setCurrentColorAccent(null);
      setCurrentColorButtons(null);

      onCloseColorSchemeDialog();
      setIsAddThemeDialog(false);

      return;
    }

    // Editing themes
    const editTheme = {
      id: selectThemeId,
      accentColor: currentColorAccent,
      buttonsMain: currentColorButtons,
      textColor: "#FFFFFF",
    };

    onSaveChangedThemes(editTheme);

    onCloseColorSchemeDialog();
  };

  const nodeHexColorPickerButtons = (
    <DropDownContainer
      directionX="right"
      manualY="62px"
      withBackdrop={false}
      isDefaultMode={false}
      open={openHexColorPickerButtons}
      clickOutsideAction={onCloseHexColorPicker}
    >
      <DropDownItem className="drop-down-item-hex">
        <HexColorPickerComponent
          id="buttons-hex"
          onCloseHexColorPicker={onCloseHexColorPicker}
          onAppliedColor={onAppliedColorButtons}
          color={appliedColorButtons}
          setColor={setAppliedColorButtons}
          viewMobile={viewMobile}
        />
      </DropDownItem>
    </DropDownContainer>
  );

  const nodeHexColorPickerAccent = (
    <DropDownContainer
      directionX="right"
      manualY="62px"
      withBackdrop={false}
      isDefaultMode={false}
      open={openHexColorPickerAccent}
      clickOutsideAction={onCloseHexColorPicker}
      viewMobile={viewMobile}
    >
      <DropDownItem className="drop-down-item-hex">
        <HexColorPickerComponent
          id="accent-hex"
          onCloseHexColorPicker={onCloseHexColorPicker}
          onAppliedColor={onAppliedColorAccent}
          color={appliedColorAccent}
          setColor={setAppliedColorAccent}
          viewMobile={viewMobile}
        />
      </DropDownItem>
    </DropDownContainer>
  );

  // const nodeAccentColor = (
  //   <div
  //     id="accent"
  //     style={{ background: currentColorAccent }}
  //     className="color-button"
  //     onClick={onClickColor}
  //   ></div>
  // );

  // const nodeButtonsColor = (
  //   <div
  //     id="buttons"
  //     style={{ background: currentColorButtons }}
  //     className="color-button"
  //     onClick={onClickColor}
  //   ></div>
  // );

  return viewMobile ? (
    <BreakpointWarning sectionName={t("Settings:Appearance")} />
  ) : !tReady ? (
    <Loader />
  ) : (
    <>
      <ModalDialogDelete
        visible={visibleDialog}
        onClose={() => setVisibleDialog(false)}
        onClickDelete={onClickDeleteModal}
      />

      <StyledComponent>
        <div className="header">{t("Common:Color")}</div>

        <div className="theme-standard">
          <div className="theme-name">{t("Common:Standard")}</div>

          <div className="theme-container">
            {appearanceTheme.map((item, index) => {
              if (index > 6) return;
              return (
                <div
                  key={index}
                  id={item.id}
                  style={{ background: item.accentColor }}
                  className="box"
                  onClick={() => onColorSelection(item)}
                >
                  {onShowCheck(item.id)}
                </div>
              );
            })}
          </div>
        </div>

        <div className="theme-custom">
          <div className="theme-name">Custom</div>

          <div className="theme-container">
            {appearanceTheme.map((item, index) => {
              if (index < 7) return;
              return (
                <div
                  key={index}
                  id={item.id}
                  style={{ background: item.accentColor }}
                  className="box"
                  onClick={() => onColorSelection(item)}
                >
                  {onShowCheck(item.id)}
                </div>
              );
            })}

            <div
              data-for="theme-add"
              data-tip={
                !abilityAddTheme
                  ? "You can only create 3 custom themes. To create a new one, you must delete one of the previous themes."
                  : null
              }
              className="box theme-add"
              onClick={() => onAddTheme()}
            />
            <Tooltip
              id="theme-add"
              offsetBottom={0}
              effect="solid"
              place="bottom"
              getContent={(dataTip) => (
                <Text fontSize="12px" noSelect>
                  {dataTip}
                </Text>
              )}
            ></Tooltip>
          </div>
        </div>

        <ColorSchemeDialog
          // nodeButtonsColor={nodeButtonsColor}
          // nodeAccentColor={nodeAccentColor}

          onClickColor={onClickColor}
          currentColorAccent={currentColorAccent}
          currentColorButtons={currentColorButtons}
          nodeHexColorPickerAccent={nodeHexColorPickerAccent}
          nodeHexColorPickerButtons={nodeHexColorPickerButtons}
          visible={showColorSchemeDialog}
          onClose={onCloseColorSchemeDialog}
          header={headerColorSchemeDialog}
          viewMobile={viewMobile}
          openHexColorPickerButtons={openHexColorPickerButtons}
          openHexColorPickerAccent={openHexColorPickerAccent}
          showSaveButtonDialog={showSaveButtonDialog}
          onSaveColorSchemeDialog={onSaveColorSchemeDialog}
        />
        <div className="header preview-header">{t("Common:Preview")}</div>
        <TabContainer elements={array_items} onSelect={onChangePreviewTheme} />

        <div className="buttons-container">
          <Button
            className="button"
            label="Save"
            onClick={onSave}
            primary
            size="small"
            isDisabled={isDisabledSaveButton}
          />

          <Button
            className="button"
            label="Edit current theme"
            onClick={onClickEdit}
            size="small"
            isDisabled={isDisabledEditButton}
          />
          {isShowDeleteButton && (
            <Button
              className="button"
              label="Delete theme"
              onClick={() => setVisibleDialog(true)}
              size="small"
              isDisabled={isDisabledDeleteButton}
            />
          )}
        </div>
      </StyledComponent>
    </>
  );
};

export default inject(({ auth }) => {
  const { settingsStore } = auth;
  const {
    appearanceTheme,
    selectedThemeId,
    sendAppearanceTheme,
    getAppearanceTheme,
    currentColorScheme,
    deleteAppearanceTheme,
  } = settingsStore;

  return {
    appearanceTheme,
    selectedThemeId,
    sendAppearanceTheme,
    getAppearanceTheme,
    currentColorScheme,
    deleteAppearanceTheme,
  };
})(
  withTranslation(["Profile", "Common", "Settings"])(
    withRouter(observer(Appearance))
  )
);
