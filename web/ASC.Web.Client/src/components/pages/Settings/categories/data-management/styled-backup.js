import styled, { css } from "styled-components";
import commonSettingsStyles from "../../utils/commonSettingsStyles";

const commonStyles = css`
  .radio-button_text {
    margin-right: 7px;
    margin-left: 8px;
    font-size: 13px;
    font-weight: 600;
  }
  .backup_radio-button {
    margin-bottom: 8px;
  }
  .backup_combo {
    margin-top: 16px;
    width: 100%;
    max-width: 350px;
    .combo-button-label {
      width: 100%;
      max-width: 350px;
    }
  }
  .backup_text-input {
    margin: 10px 0;
    width: 100%;
    max-width: 350px;
    font-size: 13px;
  }
`;

const StyledManualBackup = styled.div`
  ${commonStyles}
  .manual-backup_buttons {
    margin-top: 16px;
  }
`;

const StyledAutoBackup = styled.div`
  ${commonStyles}
  .automatic-backup_main {
    margin-bottom: 30px;
    .radio-button_text {
      font-size: 13px;
    }
  }

  .schedule-backup_combobox {
    display: inline-block;
    margin-right: 8px;
    margin-top: 8px;
  }

  .input-with-folder-path {
    margin-top: 16px;
    margin-bottom: 8px;
    width: 100%;
    max-width: 350px;
  }
`;

const StyledRestoreBackup = styled.div`
  ${commonStyles}

  .restore-source {
    // restore
    font-weight: 600;
    margin-top: 30px;
    margin-bottom: 15px;
    font-size: 15px;
  }
  .restore-warning {
    // restore
    font-weight: 600;
    margin-top: 18px;
    margin-bottom: 15px;
    font-size: 19px;
    color: #c30;
  }
  .restore-warning_link {
    // restore
    margin: 15px 0;
  }
  .restore-backup-checkbox {
    // restore
    margin-bottom: 24px;
  }
  .restore-backup-checkbox_notification {
    // restore
    margin-top: 15px;
  }
  .restore-backup_list {
    // restore
    color: #316daa;
    text-decoration: underline;
    cursor: pointer;
  }

  .restore-backup_input {
    // restore
    margin: 16px 0;
    max-width: 820px;
  }
`;

const StyledModules = styled.div`
  margin-bottom: 32px;
`;

const StyledRestoreModules = styled.div`
  .category-item-description {
    ${(props) => props.isDisabled && `color: #A3A9AE`}
  }
  .radio-button_text {
    font-weight: normal;
  }
`;
const StyledScheduleComponent = styled.div`
  .time_options {
    .drop-down_variable-size {
      width: 74px !important;
    }
  }
  .month_options {
    .drop-down_variable-size {
      width: 46px !important;
    }
  }
  .main_options {
    max-width: 820px;
  }

  .max_copies {
    width: 100%;
    max-width: 820px;
  }
  .combo-button {
    width: 100% !important;
  }
  .combo-button-label {
    max-width: 100% !important;
  }
`;

const StyledBackup = styled.div`
  .schedule-information {
    font-size: 13px;
    font-weight: 600;
    margin-bottom: 8px;
  }
  ${commonSettingsStyles}
`;

const StyledBackupList = styled.div`
  height: 100%;
  .backup-list_modal-header_wrapper_description {
    margin-bottom: 16px;
  }
  .backup-list-row-list {
    height: ${(props) => (props.displayType === "aside" ? "100vh" : "234px")};
    width: 100%;
  }
  .backup-list_modal-dialog_body {
    height: 300px;
  }
  .backup-list_options {
    display: flex;
  }
  .backup-list_trash-icon {
    width: 16px;
    height: 16px;
  }
  .backup-list_aside-header_title {
    margin: 0px;
    line-height: 56px;
    max-width: 474px;
    width: 400px;
    white-space: nowrap;
    text-overflow: ellipsis;
    overflow: hidden;
    padding-left: 16px;
    border-bottom: 1px solid #eceef1;
    margin-bottom: 16px;
  }
  .backup-list_aside-header {
    margin-bottom: 16px;
  }
  .backup-list_aside-header_description {
    margin-top: 16px;
  }
  .backup-list_modal-header_description {
    margin-bottom: 16px;
  }
  .backup-list_modal-header_description,
  .backup-list_aside-header_description {
    display: flex;
    display: contents;
    overflow-wrap: break-word;
  }
  .backup-list_clear-link {
    margin-left: 4px;
    text-decoration: underline;
  }
  .backup-list_aside-body_wrapper {
    height: calc(100% - 200px);
    width: 300px;
    padding: 0 16px;
  }
  .backup-list_aside_body {
    margin-top: 16px;
    height: 100%;
    width: 290px;
  }
  .backup-list_restore-link {
    margin-right: 16px;
    color: #a3a9ae;
    max-width: 200px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .backup-list_restore-link:hover {
    text-decoration: none;
    font-weight: 600;
    color: #657077;
  }
  .backup-list_trash-icon:hover {
    path {
      fill: #657077;
    }
  }
  .backup-list_trash-icon,
  .backup-list_restore-link {
    cursor: pointer;
  }
  .backup-list_trash-icon {
    margin-top: 1px;
  }
`;
export {
  StyledModules,
  StyledRestoreBackup,
  StyledScheduleComponent,
  StyledBackup,
  StyledRestoreModules,
  StyledBackupList,
  StyledManualBackup,
  StyledAutoBackup,
};
