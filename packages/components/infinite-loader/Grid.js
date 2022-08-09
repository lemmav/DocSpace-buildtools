import React, { memo, useCallback, useEffect, useRef } from "react";
import AutoSizer from "react-virtualized-auto-sizer";
import InfiniteLoader from "react-window-infinite-loader";
import { VariableSizeList as List, areEqual } from "react-window";
//import Scroll from "./Scroll";

const GridComponent = ({
  hasMoreFiles,
  filesLength,
  itemCount,
  loadMoreItems,
  onScroll,
  countTilesInRow,
  selectedFolderId,
  children,
  className,
  listRef,
}) => {
  const gridRef = useRef(null);

  useEffect(() => {
    //TODO: inf-scroll it is slow
    //console.log("resetAfterIndex");

    gridRef?.current?.resetAfterIndex(0);
  }, [selectedFolderId]);

  const isItemLoaded = useCallback(
    (index) => {
      return !hasMoreFiles || index * countTilesInRow < filesLength;
    },
    [filesLength, hasMoreFiles, countTilesInRow]
  );

  const renderTile = memo(({ index, style }) => {
    return <div style={style}>{children[index]}</div>;
  }, areEqual);

  const getItemSize = (index) => {
    const itemClassNames = children[index]?.props?.className;
    const isFile = itemClassNames?.includes("isFile");
    const isFolder = itemClassNames?.includes("isFolder");

    const horizontalGap = 16;
    const verticalGap = 14;
    const headerMargin = 15;

    const folderHeight = 64 + verticalGap;
    const fileHeight = 220 + horizontalGap;
    const titleHeight = 20 + headerMargin;

    return isFolder ? folderHeight : isFile ? fileHeight : titleHeight;
  };

  const renderGrid = ({ height, width }) => {
    return (
      <InfiniteLoader
        isItemLoaded={isItemLoaded}
        itemCount={itemCount}
        loadMoreItems={loadMoreItems}
      >
        {({ onItemsRendered, ref }) => (
          <List
            onScroll={onScroll}
            className={className}
            height={height}
            width={width}
            itemCount={children.length}
            itemSize={getItemSize}
            onItemsRendered={onItemsRendered}
            ref={(refList) => {
              ref(listRef);
              gridRef.current = refList;
              listRef.current = refList;
            }}
            //outerElementType={Scroll}
            overscanCount={5} //TODO: inf-scroll
            style={{ height: "100% !important" }}
          >
            {renderTile}
          </List>
        )}
      </InfiniteLoader>
    );
  };

  //console.log("GridComponent render");

  return <AutoSizer>{renderGrid}</AutoSizer>;
};

export default GridComponent;
