import React from "react";
import Text from "@docspace/components/text";
import MultipleSpaces from "./sub-components/MultipleSpaces";
import { SpaceContainer } from "./StyledSpaces";
import ConfigurationSection from "./sub-components/ConfigurationSection";
import { observer } from "mobx-react";
import { useStore } from "SRC_DIR/store";

const Spaces = () => {
  const [isLoading, setIsLoading] = React.useState<boolean>(true);

  const { spacesStore, authStore } = useStore();

  const { initStore, isConnected, portals } = spacesStore;

  React.useEffect(() => {
    const fetchData = async () => {
      await initStore();
      setIsLoading(false);
    };

    fetchData();
  }, []);

  if (isLoading) return <h1>Loading</h1>;

  return (
    <SpaceContainer>
      <div className="spaces-header">
        <Text>
          Use this section to create several spaces and make them accessible for
          your users
        </Text>
      </div>
      {isConnected && portals.length > 0 ? (
        <MultipleSpaces portals={portals} />
      ) : (
        <ConfigurationSection />
      )}
    </SpaceContainer>
  );
};

export default observer(Spaces);
