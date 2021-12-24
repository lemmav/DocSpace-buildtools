import Base from '@appserver/components/themes/base';
import styled, { keyframes, css } from 'styled-components';

const StyledCircleWrap = styled.div`
  width: 54px;
  height: 54px;
  background: none;
  border-radius: 50%;
  cursor: pointer;
`;

const rotate360 = keyframes`
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
`;

StyledCircleWrap.defaultProps = { theme: Base };

const StyledCircle = styled.div`
  .circle__mask,
  .circle__fill {
    width: 54px;
    height: 54px;
    position: absolute;
    border-radius: 50%;
  }

  ${(props) =>
    props.percent > 0
      ? css`
          .circle__mask {
            clip: rect(0px, 54px, 54px, 27px);
          }

          .circle__fill {
            animation: fill-rotate ease-in-out none;
            transform: rotate(${(props) => props.percent * 1.8}deg);
          }
        `
      : css`
          .circle__fill {
            animation: ${rotate360} 2s linear infinite;
            transform: translate(0);
          }
        `}

  .circle__mask .circle__fill {
    clip: rect(0px, 27px, 54px, 0px);
    background-color: ${(props) => props.theme.floatingButton.color};
  }

  .circle__mask.circle__full {
    animation: fill-rotate ease-in-out none;
    transform: rotate(${(props) => props.percent * 1.8}deg);
  }

  @keyframes fill-rotate {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(${(props) => props.percent * 1.8}deg);
    }
  }
`;

const StyledFloatingButton = styled.div`
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: ${(props) => props.theme.floatingButton.backgroundColor};
  box-shadow: ${(props) => props.theme.floatingButton.boxShadow};
  text-align: center;
  margin: 3px;
  position: absolute;
`;

StyledFloatingButton.defaultProps = { theme: Base };

const IconBox = styled.div`
  padding-top: 12px;
  svg {
    path {
      fill: ${(props) => props.theme.floatingButton.fill};
    }
  }
`;

IconBox.defaultProps = { theme: Base };

const StyledAlertIcon = styled.div`
  position: absolute;
  width: 12px;
  height: 12px;
  left: 26px;
  top: -10px;
`;

export { StyledCircleWrap, StyledCircle, StyledFloatingButton, StyledAlertIcon, IconBox };
