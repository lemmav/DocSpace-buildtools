import React from "react";
import Text from "@docspace/components/text";
import Link from "@docspace/components/link";
import TextInput from "@docspace/components/text-input";
import FieldContainer from "@docspace/components/field-container";

const LinkBlock = (props) => {
  const {
    t,
    isLoading,
    shareLink,
    linkNameValue,
    setLinkNameValue,
    linkValue,
    setLinkValue,
    isLinkNameValid,
    setIsLinkNameValid,
  } = props;

  const onChangeLinkName = (e) => {
    setLinkNameValue(e.target.value);
    setIsLinkNameValid(true);
  };

  const onShortenClick = () => {
    alert("api in progress");
    // setLinkValue
  };

  return (
    <div className="edit-link_link-block">
      <Text className="edit-link-text" fontSize="13px" fontWeight={600}>
        {t("LinkName")}
      </Text>
      <FieldContainer
        isVertical
        hasError={!isLinkNameValid}
        errorMessage={t("Common:RequiredField")}
        className="edit-link_password-block"
      >
        <TextInput
          scale
          size="base"
          withBorder
          isAutoFocussed={false}
          className="edit-link_name-input"
          value={linkNameValue}
          onChange={onChangeLinkName}
          placeholder={t("ExternalLink")}
          isDisabled={isLoading}
          hasError={!isLinkNameValid}
        />
      </FieldContainer>

      <TextInput
        scale
        size="base"
        withBorder
        isDisabled
        isReadOnly
        className="edit-link_link-input"
        value={linkValue}
        placeholder={t("ExternalLink")}
      />

      <Link
        fontSize="13px"
        fontWeight={600}
        isHovered
        type="action"
        isDisabled={isLoading}
        onClick={onShortenClick}
      >
        {t("Shorten")}
      </Link>
    </div>
  );
};

export default LinkBlock;
