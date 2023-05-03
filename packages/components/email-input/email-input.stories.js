import React, { useState } from "react";
import { EmailSettings } from "../utils/email";
import EmailInput from "./";

export default {
  title: "Components/EmailInput",
  component: EmailInput,
  argTypes: {
    allowDomainPunycode: {
      description: "History property. Required to be passed into emailSettings",
    },
    allowLocalPartPunycode: {
      description: "History property. Required to be passed into emailSettings",
    },
    allowDomainIp: {
      description: "History property. Required to be passed into emailSettings",
    },
    allowStrictLocalPart: {
      description: "History property. Required to be passed into emailSettings",
    },
    allowSpaces: {
      description: "History property. Required to be passed into emailSettings",
    },
    allowName: {
      description: "History property. Required to be passed into emailSettings",
    },
    allowLocalDomainName: {
      description: "History property. Required to be passed into emailSettings",
    },
  },
};

const Template = ({
  allowDomainPunycode,
  allowLocalPartPunycode,
  allowDomainIp,
  allowStrictLocalPart,
  allowSpaces,
  allowName,
  allowLocalDomainName,
  ...rest
}) => {
  const [emailValue, setEmailValue] = useState("");

  const onChangeHandler = (value) => {
    setEmailValue(value);
  };
  const settings = EmailSettings.parse({
    allowDomainPunycode,
    allowLocalPartPunycode,
    allowDomainIp,
    allowStrictLocalPart,
    allowSpaces,
    allowName,
    allowLocalDomainName,
  });
  return (
    <div style={{ margin: "7px" }}>
      <EmailInput
        {...rest}
        value={emailValue}
        emailSettings={settings}
        onValidateInput={(isEmailValid) => rest.onValidateInput(isEmailValid)}
        onChange={(e) => {
          rest.onChange(e.target.value);
          onChangeHandler(e.target.value);
        }}
      />
    </div>
  );
};

export const Default = Template.bind({});
Default.args = {
  allowDomainPunycode: false,
  allowLocalPartPunycode: false,
  allowDomainIp: false,
  allowSpaces: false,
  allowName: false,
  allowLocalDomainName: false,
  allowStrictLocalPart: true,
  placeholder: "Input email",
  size: "base",
};
