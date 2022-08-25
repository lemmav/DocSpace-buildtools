﻿// (c) Copyright Ascensio System SIA 2010-2022
//
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
//
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
//
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
//
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
//
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
//
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode


namespace ASC.Web.Api.Controllers;

[Scope]
[DefaultRoute]
[ApiController]
[ControllerName("portal")]
public class PaymentController : ControllerBase
{
    private readonly ApiContext _apiContext;
    private readonly UserManager _userManager;
    private readonly TenantManager _tenantManager;
    private readonly ITariffService _tariffService;
    private readonly SecurityContext _securityContext;
    private readonly GeolocationHelper _geolocationHelper;
    protected Tenant Tenant { get { return _apiContext.Tenant; } }

    public PaymentController(
        ApiContext apiContext,
        UserManager userManager,
        TenantManager tenantManager,
        ITariffService tariffService,
        SecurityContext securityContext,
        GeolocationHelper geolocationHelper
        )
    {
        _apiContext = apiContext;
        _userManager = userManager;
        _tenantManager = tenantManager;
        _tariffService = tariffService;
        _securityContext = securityContext;
        _geolocationHelper = geolocationHelper;
    }

    [HttpPut("payment/url")]
    public Uri GetPaymentUrl(PaymentUrlRequestsDto inDto)
    {
        if (_tariffService.GetPayments(Tenant.Id).Any()
            || !_userManager.GetUsers(_securityContext.CurrentAccount.ID).IsAdmin(_userManager))
        {
            return null;
        }

        var currency = GetCurrencyFromRequest();

        return _tariffService.GetShoppingUri(Tenant.Id, currency,
            Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName,
            _userManager.GetUsers(_securityContext.CurrentAccount.ID).Email,
            inDto.Quantity,
            inDto.BackUrl);
    }

    [HttpPut("payment/update")]
    public bool PaymentUpdate(PaymentUrlRequestsDto inDto)
    {
        if (!_tariffService.GetPayments(Tenant.Id).Any()
            || !_userManager.GetUsers(_securityContext.CurrentAccount.ID).IsAdmin(_userManager))
        {
            return false;
        }

        return _tariffService.PaymentChange(Tenant.Id, inDto.Quantity);
    }

    [HttpGet("payment/account")]
    public Uri GetPaymentAccount(string backUrl)
    {
        var payerId = _tariffService.GetTariff(Tenant.Id).CustomerId;

        if (_securityContext.CurrentAccount.ID != payerId
            && _securityContext.CurrentAccount.ID != Tenant.OwnerId)
            return null;

        return _tariffService.GetAccountLink(Tenant.Id, backUrl);
    }

    [HttpGet("payment/prices")]
    public object GetPrices()
    {
        var currency = GetCurrencyFromRequest();
        var result = _tenantManager.GetProductPriceInfo()
            .ToDictionary(pr => pr.Key, pr => pr.Value.ContainsKey(currency) ? pr.Value[currency] : 0);
        return result;
    }
    private string GetCurrencyFromRequest()
    {
        var regionInfo = new RegionInfo("US");
        var geoinfo = _geolocationHelper.GetIPGeolocationFromHttpContext();
        if (!string.IsNullOrEmpty(geoinfo.Key))
        {
            try
            {
                regionInfo = new RegionInfo(geoinfo.Key);
            }
            catch (Exception)
            {
            }
        }
        return regionInfo.ISOCurrencySymbol;
    }
}
