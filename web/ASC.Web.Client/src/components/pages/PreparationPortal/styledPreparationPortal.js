import styled from "styled-components";

const StyledBodyPreparationPortal = styled.div`
  margin-bottom: 24px;
  display: flex;
  width: 100%;
  max-width: ${(props) => (props.errorMessage ? "560px" : "480px")};
  padding: 0 24px;
  box-sizing: border-box;
  align-items: center;
  position: relative;

  .preparation-portal_progress-bar {
    border-radius: 2px;
    margin-right: 8px;
    width: 100%;

    height: 24px;
    background-color: #f3f4f4;
  }
  .preparation-portal_progress-line {
    border-radius: inherit;
    width: ${(props) => props.percent}%;
    background: #439ccd;
    height: inherit;
    transition-property: width;
    transition-duration: 0.9s;
    background: #1f97ca;
  }
  .preparation-portal_percent {
    position: absolute;
    right: 50%;
    ${(props) => props.percent > 50 && "color: white"}
  }
`;

const StyledPreparationPortal = styled.div`
  width: 100%;

  #header {
    font-size: 23px;
  }
  #text {
    color: #a3a9ae;
    font-size: 13px;
    line-height: 20px;
    max-width: 480px;
  }
`;
export { StyledBodyPreparationPortal, StyledPreparationPortal };
