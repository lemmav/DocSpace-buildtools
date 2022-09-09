import React from "react";
import {
  loadComponent,
  useDynamicScript,
} from "../components/DynamicComponent";
import { STUDIO_SCOPE, STUDIO_REMOTE_ENTRY_URL } from "../helpers/constants";

function useMfScripts() {
  const [isInitialized, setIsInitialized] = React.useState(false);
  const [isError, setIsError] = React.useState(false);

  const { ready: studioReady, failed: studioFailed } = useDynamicScript({
    id: STUDIO_SCOPE,
    url: STUDIO_REMOTE_ENTRY_URL,
  });

  React.useEffect(() => {
    if (studioReady) {
      initMfScripts();
    }

    if (studioFailed) {
      setIsError(true);
      setIsInitialized(false);
    }
  }, [studioReady]);

  const initMfScripts = async () => {
    const SharingDialog = await loadComponent(
      STUDIO_SCOPE,
      "./SharingDialog"
    )();
    const filesUtils = await loadComponent(STUDIO_SCOPE, "./utils")();
    const authStore = await loadComponent(STUDIO_SCOPE, "./store")();

    window.filesUtils = filesUtils;
    window.SharingDialog = SharingDialog.default;
    window.authStore = authStore.default;

    setIsInitialized(true);
    setIsError(false);
  };

  return [isInitialized, isError];
}

export default useMfScripts;
