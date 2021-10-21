import React, { memo } from "react";
import { withRouter } from "react-router";
import Loaders from "@appserver/common/components/Loaders";
import VersionRow from "./VersionRow";
import { inject, observer } from "mobx-react";
import { VariableSizeList as List, areEqual } from "react-window";
import AutoSizer from "react-virtualized-auto-sizer";
import CustomScrollbarsVirtualList from "@appserver/components/scrollbar/custom-scrollbars-virtual-list";
import StyledVersionList from "./StyledVersionList";
class SectionBodyContent extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      isRestoreProcess: false,
      rowSizes: {},
    };
    this.listKey = 0;
    this.listRef = React.createRef();
  }

  componentDidMount() {
    const { match, setFirstLoad, versions } = this.props;
    const fileId = match.params.fileId || this.props.fileId;

    if (fileId && fileId !== this.props.fileId) {
      this.getFileVersions(fileId);
      setFirstLoad(false);
    }
  }

  getFileVersions = (fileId) => {
    const { fetchFileVersions, setIsLoading } = this.props;
    setIsLoading(true);
    fetchFileVersions(fileId).then(() => setIsLoading(false));
  };

  onSetRestoreProcess = (isRestoreProcess) => {
    console.log("onSetRestoreProcess", isRestoreProcess);

    this.setState({
      isRestoreProcess,
    });
  };
  onUpdateHeight = (i, itemHeight) => {
    if (this.listRef.current) {
      this.listRef.current.resetAfterIndex(i);
    }

    this.setState((prevState) => ({
      rowSizes: {
        ...prevState.rowSizes,
        [i]: itemHeight + 24, //composed of itemHeight = clientHeight of div and padding-top = 12px and padding-bottom = 12px
      },
    }));
  };

  getSize = (i) => {
    return this.state.rowSizes[i] ? this.state.rowSizes[i] : 66;
  };

  renderRow = memo(({ index, style }) => {
    const { versions, culture } = this.props;

    const prevVersion = versions[index > 0 ? index - 1 : index].versionGroup;
    let isVersion = true;

    if (index > 0 && prevVersion === versions[index].versionGroup) {
      isVersion = false;
    }
    console.log("render row", this.state, "versions.length", versions.length);

    return (
      <div style={style}>
        <VersionRow
          getFileVersions={this.getFileVersions}
          isVersion={isVersion}
          key={`${versions[index].id}-${index}`}
          info={versions[index]}
          versionsListLength={versions.length}
          index={index}
          culture={culture}
          onSetRestoreProcess={this.onSetRestoreProcess}
          onUpdateHeight={this.onUpdateHeight}
        />
      </div>
    );
  }, areEqual);
  render() {
    const { versions, isLoading } = this.props;

    const renderList = ({ height, width }) => {
      console.log(
        "render list",
        this.state,
        "versions.length",
        versions.length
      );
      return (
        <StyledVersionList isRestoreProcess={this.state.isRestoreProcess}>
          <List
            ref={this.listRef}
            className="List"
            height={height}
            width={width}
            itemSize={this.getSize}
            itemCount={versions.length}
            itemData={versions}
            outerElementType={CustomScrollbarsVirtualList}
          >
            {this.renderRow}
          </List>
        </StyledVersionList>
      );
    };
    console.log("versions", versions);
    return versions && !isLoading ? (
      <div style={{ height: "100%", width: "100%" }}>
        <AutoSizer>{renderList}</AutoSizer>
      </div>
    ) : (
      <div className="loader-history-rows" style={{ paddingRight: "16px" }}>
        <Loaders.HistoryRows title="version-history-body-loader" />
      </div>
    );
  }
}

export default inject(({ auth, filesStore, versionHistoryStore }) => {
  const { setFirstLoad, setIsLoading, isLoading } = filesStore;
  const {
    versions,
    fetchFileVersions,
    fileId,
    setVerHistoryFileId,
  } = versionHistoryStore;

  return {
    culture: auth.settingsStore.culture,
    isTabletView: auth.settingsStore.isTabletView,
    isLoading,
    versions,
    fileId,

    setFirstLoad,
    setIsLoading,
    fetchFileVersions,
    setVerHistoryFileId,
  };
})(withRouter(observer(SectionBodyContent)));
