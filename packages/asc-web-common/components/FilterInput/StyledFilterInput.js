import Base from '@appserver/components/themes/base';
import styled, { css } from 'styled-components';

const StyledFilterInput = styled.div`
  width: 100%;
  min-width: 255px;
  display: flex;
  &:after {
    content: ' ';
    display: block;
    height: 0;
    clear: both;
    visibility: hidden;
  }

  .styled-search-input {
    display: block;
    width: 100%;

    .search-input-block {
      & > input {
        height: 30px;
        line-height: 30px;
      }
    }
  }

  .styled-filter-block {
    display: flex;
    #filter-button {
      svg {
        height: 25px;

        path:first-child {
          fill: ${(props) => props.theme.filterInput.filterButton.fill} !important;
          stroke: ${(props) => props.theme.filterInput.filterButton.stroke} !important;
        }

        path:not(:first-child) {
          fill: ${(props) => props.theme.filterInput.filterButton.fillSecond} !important;
          /* stroke: ${(props) => props.theme.filterInput.filterButton.stroke} !important; */
        }

        /* path:not(:first-child) {
          stroke: ${(props) => props.theme.filterInput.filterButton.stroke};
        } */
      }

      /* stroke: ${(props) => props.theme.filterInput.filterButton.stroke};
      div:active {
        svg path:first-child {
          fill: ${(props) => props.theme.filterInput.filterButton.fill};
          stroke: ${(props) => props.theme.filterInput.filterButton.stroke};
        }
      }
      div:first-child:hover {
        svg path:not(:first-child) {
          stroke: ${(props) => props.theme.filterInput.filterButton.stroke};
        }
      } */
    }
  }

  .styled-close-button {
    margin-left: 7px;
    margin-top: -1px;
  }

  .styled-filter-block {
    display: flex;
    align-items: center;
  }

  .styled-combobox {
    display: inline-block;
    background: transparent;
    max-width: 185px;
    cursor: pointer;
    vertical-align: middle;
    margin-top: -2px;
    > div:first-child {
      width: auto;
      padding-left: 4px;
    }
    > div:last-child {
      max-width: 220px;
    }
    .combo-button-label {
      color: ${(props) => props.theme.filterInput.comboButtonLabelColor};
    }
  }

  .styled-filter-name {
    line-height: 18px;
    margin-left: 5px;
    margin-top: -2px;
    margin-right: 4px;
  }

  .styled-hide-filter {
    display: inline-block;
    height: 100%;

    .hide-filter-drop-down {
      .combo-button-label {
        ${(props) => (props.isAllItemsHide ? 'margin-top: 2px;' : null)}
      }
    }
  }

  .dropdown-style {
    position: relative;

    .backdrop-active {
      z-index: 190;
    }

    .drop-down {
      padding: 16px;
    }
  }

  .styled-sort-combobox {
    display: block;
    width: fit-content;
    margin-left: 8px;

    ${(props) =>
      (props.isMobile || props.smallSectionWidth) &&
      `
          width: 50px;
          .optionalBlock ~ div:first-child{
              opacity: 0
          }
      `}

    .combo-button-label {
      color: ${(props) => props.theme.filterInput.comboButtonLabelColorTwo};
    }
  }

  .view-selector-button {
    display: flex;
    float: left;

    margin-left: 8px;

    @media (max-width: 460px) {
      display: none;
    }
  }
`;

StyledFilterInput.defaultProps = { theme: Base };

export const StyledViewSelector = styled.div`
  border: 1px solid
    ${(props) =>
      props.isDisabled
        ? props.theme.filterInput.viewSelector.disabledBorder
        : props.theme.filterInput.viewSelector.border};
  border-radius: 3px;
  padding: 7px;
  ${(props) =>
    props.isDisabled &&
    `background-color: ${props.theme.filterInput.viewSelector.disabledBackground}`}

  svg {
    pointer-events: none;
  }

  &.active {
    background-color: ${(props) => props.theme.filterInput.viewSelector.activeBackground};
    border-color: ${(props) => props.theme.filterInput.viewSelector.activeBorder};
  }

  &:hover {
    ${(props) =>
      !props.isDisabled &&
      `background-color: ${props.theme.filterInput.viewSelector.activeBackground};`}
    ${(props) =>
      !props.isDisabled && `border-color: ${props.theme.filterInput.viewSelector.activeBorder};`}
  }

  &:first-child {
    border-right: none;
    border-top-right-radius: 0;
    border-bottom-right-radius: 0;
  }

  &:last-child {
    border-left: none;
    border-top-left-radius: 0;
    border-bottom-left-radius: 0;
  }
`;

StyledViewSelector.defaultProps = { theme: Base };

export const StyledFilterItem = styled.div`
  display: ${(props) => (props.block ? 'flex' : 'inline-block')};
  margin-bottom: ${(props) => (props.block ? '8px' : '0')};
  position: relative;
  height: 25px;
  margin-right: 2px;
  border: ${(props) => props.theme.filterInput.filterItem.border};
  border-radius: 3px;
  background-color: ${(props) => props.theme.filterInput.filterItem.backgroundColor};
  padding-right: 22px;

  font-weight: 600;
  font-size: 13px;
  line-height: 15px;
  box-sizing: border-box;
  color: ${(props) => props.theme.filterInput.filterItem.color};

  &:last-child {
    margin-bottom: 0;
  }
`;

StyledFilterItem.defaultProps = { theme: Base };

export const StyledFilterItemContent = styled.div`
  display: flex;
  padding: 4px 4px 2px 7px;
  width: max-content;
  user-select: none;
  color: ${(props) => props.theme.filterInput.content.color};
  ${(props) =>
    props.isOpen &&
    !props.isDisabled &&
    css`
      background: ${props.theme.filterInput.content.background};
    `}
  ${(props) =>
    !props.isDisabled &&
    css`
      &:active {
        background: ${props.theme.filterInput.content.background};
      }
    `}
`;

StyledFilterItemContent.defaultProps = { theme: Base };

export const StyledCloseButtonBlock = styled.div`
  display: flex;
  cursor: ${(props) => (props.isDisabled || !props.isClickable ? 'default' : 'pointer')};
  align-items: center;
  position: absolute;
  height: 100%;
  width: 25px;
  border-left: ${(props) => props.theme.filterInput.closeButton.borderLeft};
  right: 0;
  top: 0;
  background-color: ${(props) => props.theme.filterInput.closeButton.background};
  ${(props) =>
    !props.isDisabled &&
    css`
      &:active {
        background: ${props.theme.filterInput.closeButton.activeBackground};
        svg path:first-child {
          fill: ${props.theme.filterInput.closeButton.activeFill};
        }
      }

      :hover {
        .styled-close-button {
          svg {
            path {
              fill: ${props.theme.filterInput.closeButton.hoverFill};
            }
          }
        }
      }
    `}
`;

StyledCloseButtonBlock.defaultProps = { theme: Base };

export const Caret = styled.div`
  width: 7px;
  position: absolute;
  right: 6px;
  transform: ${(props) => (props.isOpen ? 'rotate(180deg)' : 'rotate(0)')};
  top: ${(props) => (props.isOpen ? '2px' : '0')};
`;

export const StyledHideFilterButton = styled.div`
  box-sizing: border-box;
  display: flex;
  position: relative;
  align-items: center;
  font-weight: 600;
  font-size: 16px;
  height: 25px;
  border: ${(props) => props.theme.filterInput.hideButton.border};
  border-radius: 3px;
  background-color: ${(props) => props.theme.filterInput.hideButton.background};
  padding: 0 20px 0 9px;
  margin-right: 2px;
  cursor: ${(props) => (props.isDisabled ? 'default' : 'pointer')};
  font-family: Open Sans;
  font-style: normal;

  :hover {
    border-color: ${(props) =>
      props.isDisabled
        ? props.theme.filterInput.hideButton.disabledHoverBorder
        : props.theme.filterInput.hideButton.hoverBorder};
  }
  :active {
    background-color: ${(props) =>
      props.isDisabled
        ? props.theme.filterInput.hideButton.disabledActiveBackground
        : props.theme.filterInput.hideButton.activeBackground};
  }
`;

StyledHideFilterButton.defaultProps = { theme: Base };

export const StyledIconButton = styled.div`
  transform: ${(state) => (!state.sortDirection ? 'scale(1, -1)' : 'scale(1)')};
`;

export const StyledIconWrapper = styled.div`
  display: inline-flex;
  width: 32px;
  height: 100%;
`;

export default StyledFilterInput;
