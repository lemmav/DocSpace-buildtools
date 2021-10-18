import React from 'react';
import PropTypes from 'prop-types';
import styled from 'styled-components';

import Text from '@appserver/components/text';

import DefaultIcon from '../svg/default.react.svg';
import RootIcon from '../svg/root.react.svg';
import DefaultTabletIcon from '../svg/default.tablet.react.svg';
import RootTabletIcon from '../svg/root.tablet.react.svg';

import { isMobile } from 'react-device-detect';
import { tablet, isTablet, isMobile as IsMobileUtils } from '@appserver/components/utils/device';

const StyledItem = styled.div`
  height: auto;
  width: auto !important;
  position: relative;
  display: grid;
  align-items: end;
  grid-template-columns: 17px auto;
  cursor: pointer;
  padding: ${isMobile ? '0px 16px' : '0px 24px'};

  @media ${tablet} {
    padding: 0px 16px;
  }
`;

const StyledIconWrapper = styled.div`
  width: 19px;
  display: flex;
  align-items: ${(props) => (props.isRoot ? 'center' : 'flex-end')};
  justify-content: center;
`;

const StyledText = styled(Text)`
  margin-left: 10px;
  position: relative;
  bottom: ${(props) => (props.isRoot ? '-2px' : '-1px')};
`;

const Item = ({ id, title, isRoot, onClick, ...rest }) => {
  const onClickAvailable = () => {
    onClick && onClick(id);
  };

  React;

  return (
    <StyledItem id={id} isRoot={isRoot} onClick={onClickAvailable} {...rest}>
      <StyledIconWrapper isRoot={isRoot}>
        {isMobile || isTablet() || IsMobileUtils() ? (
          isRoot ? (
            <RootTabletIcon />
          ) : (
            <DefaultTabletIcon />
          )
        ) : isRoot ? (
          <RootIcon />
        ) : (
          <DefaultIcon />
        )}
      </StyledIconWrapper>
      <StyledText
        isRoot={isRoot}
        fontWeight={isRoot ? '600' : '400'}
        isRoot={isRoot}
        fontSize={'15px'}
        truncate={true}>
        {title}
      </StyledText>
    </StyledItem>
  );
};

Item.propTypes = {
  id: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  title: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  isRoot: PropTypes.bool,
  onClick: PropTypes.func,
};

export default React.memo(Item);
