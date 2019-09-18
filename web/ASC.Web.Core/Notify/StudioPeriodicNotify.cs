/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.PublicResources;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace ASC.Web.Studio.Core.Notify
{
    public class StudioPeriodicNotify
    {
        public static void SendSaasLetters(INotifyClient client, string senderName, DateTime scheduleDate, IServiceProvider serviceProvider)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var now = scheduleDate.Date;
            const string dbid = "webstudio";

            log.Info("Start SendSaasTariffLetters");

            var activeTenants = new List<Tenant>();
            var monthQuotasIds = new List<int>();

            using (var scope = serviceProvider.CreateScope())
            {
                var tenantManager = scope.ServiceProvider.GetService<TenantManager>();

                activeTenants = tenantManager.GetTenants().ToList();

                if (activeTenants.Count <= 0)
                {
                    log.Info("End SendSaasTariffLetters");
                    return;
                }

                monthQuotasIds = tenantManager.GetTenantQuotas()
                                .Where(r => !r.Trial && r.Visible && !r.Year && !r.Year3 && !r.Free && !r.NonProfit)
                                .Select(q => q.Id)
                                .ToList();
            }


            foreach (var tenant in activeTenants)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var tenantManager = scope.ServiceProvider.GetService<TenantManager>();

                    tenantManager.SetCurrentTenant(tenant.TenantId);

                    var userManager = scope.ServiceProvider.GetService<UserManager>();
                    var studioNotifyHelper = scope.ServiceProvider.GetService<StudioNotifyHelper>();
                    var tariff = CoreContext.PaymentManager.GetTariff(tenant.TenantId);
                    var quota = tenantManager.GetTenantQuota(tenant.TenantId);
                    var tenantExtra = scope.ServiceProvider.GetService<TenantExtra>();
                    var authContext = scope.ServiceProvider.GetService<AuthContext>();

                    var duedate = tariff.DueDate.Date;
                    var delayDuedate = tariff.DelayDueDate.Date;

                    INotifyAction action = null;
                    var paymentMessage = true;

                    var toadmins = false;
                    var tousers = false;
                    var toowner = false;

                    var coupon = string.Empty;

                    Func<string> greenButtonText = () => string.Empty;
                    string blueButtonText() => WebstudioNotifyPatternResource.ButtonRequestCallButton;
                    var greenButtonUrl = string.Empty;

                    Func<string> tableItemText1 = () => string.Empty;
                    Func<string> tableItemText2 = () => string.Empty;
                    Func<string> tableItemText3 = () => string.Empty;
                    Func<string> tableItemText4 = () => string.Empty;
                    Func<string> tableItemText5 = () => string.Empty;
                    Func<string> tableItemText6 = () => string.Empty;
                    Func<string> tableItemText7 = () => string.Empty;

                    var tableItemUrl1 = string.Empty;
                    var tableItemUrl2 = string.Empty;
                    var tableItemUrl3 = string.Empty;
                    var tableItemUrl4 = string.Empty;
                    var tableItemUrl5 = string.Empty;
                    var tableItemUrl6 = string.Empty;
                    var tableItemUrl7 = string.Empty;

                    var tableItemImg1 = string.Empty;
                    var tableItemImg2 = string.Empty;
                    var tableItemImg3 = string.Empty;
                    var tableItemImg4 = string.Empty;
                    var tableItemImg5 = string.Empty;
                    var tableItemImg6 = string.Empty;
                    var tableItemImg7 = string.Empty;

                    Func<string> tableItemComment1 = () => string.Empty;
                    Func<string> tableItemComment2 = () => string.Empty;
                    Func<string> tableItemComment3 = () => string.Empty;
                    Func<string> tableItemComment4 = () => string.Empty;
                    Func<string> tableItemComment5 = () => string.Empty;
                    Func<string> tableItemComment6 = () => string.Empty;
                    Func<string> tableItemComment7 = () => string.Empty;

                    Func<string> tableItemLearnMoreText1 = () => string.Empty;
                    string tableItemLearnMoreText2() => string.Empty;
                    string tableItemLearnMoreText3() => string.Empty;
                    string tableItemLearnMoreText4() => string.Empty;
                    string tableItemLearnMoreText5() => string.Empty;
                    string tableItemLearnMoreText6() => string.Empty;
                    string tableItemLearnMoreText7() => string.Empty;

                    var tableItemLearnMoreUrl1 = string.Empty;
                    var tableItemLearnMoreUrl2 = string.Empty;
                    var tableItemLearnMoreUrl3 = string.Empty;
                    var tableItemLearnMoreUrl4 = string.Empty;
                    var tableItemLearnMoreUrl5 = string.Empty;
                    var tableItemLearnMoreUrl6 = string.Empty;
                    var tableItemLearnMoreUrl7 = string.Empty;


                    if (quota.Trial)
                    {
                        #region After registration letters

                        #region 3 days after registration to admins SAAS TRIAL + only 1 user

                        if (tenant.CreatedDateTime.Date.AddDays(3) == now && userManager.GetUsers().Count() == 1)
                        {
                            action = Actions.SaasAdminInviteTeammatesV10;
                            paymentMessage = false;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonInviteRightNow;
                            greenButtonUrl = string.Format("{0}/products/people/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 5 days after registration to admins SAAS TRAIL + without activity in 1 or more days

                        else if (tenant.CreatedDateTime.Date.AddDays(5) == now)
                        {
                            List<DateTime> datesWithActivity;

                            var query = new SqlQuery("feed_aggregate")
                                .Select(new SqlExp("cast(created_date as date) as short_date"))

                                .Where("tenant", tenantManager.GetCurrentTenant().TenantId)
                                .Where(Exp.Le("created_date", now.AddDays(-1)))
                                .GroupBy("short_date");

                            using (var db = new DbManager(dbid))
                            {
                                datesWithActivity = db
                                    .ExecuteList(query)
                                    .ConvertAll(r => Convert.ToDateTime(r[0]));
                            }

                            if (datesWithActivity.Count < 5)
                            {
                                action = Actions.SaasAdminWithoutActivityV10;
                                paymentMessage = false;
                                toadmins = true;
                            }
                        }

                        #endregion

                        #region 7 days after registration to admins and users SAAS TRIAL

                        else if (tenant.CreatedDateTime.Date.AddDays(7) == now)
                        {
                            action = Actions.SaasAdminUserDocsTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-formatting-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_formatting_hdr;
                            tableItemComment1 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_formatting;
                            tableItemLearnMoreUrl1 = studioNotifyHelper.Helplink + "onlyoffice-editors/index.aspx";
                            tableItemLearnMoreText1 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg2 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-share-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_share_hdr;
                            tableItemComment2 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_share;

                            tableItemImg3 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-coediting-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_coediting_hdr;
                            tableItemComment3 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_coediting;

                            tableItemImg4 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-review-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_review_hdr;
                            tableItemComment4 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_review;

                            tableItemImg5 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-3rdparty-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_3rdparty_hdr;
                            tableItemComment5 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_3rdparty;

                            tableItemImg6 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-attach-100.png";
                            tableItemText6 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_attach_hdr;
                            tableItemComment6 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_attach;

                            tableItemImg7 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-apps-100.png";
                            tableItemText7 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_apps_hdr;
                            tableItemComment7 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_apps;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;
                            greenButtonUrl = string.Format("{0}/products/files/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 14 days after registration to admins and users SAAS TRIAL

                        else if (tenant.CreatedDateTime.Date.AddDays(14) == now)
                        {
                            action = Actions.SaasAdminUserComfortTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;
                        }

                        #endregion

                        #region 21 days after registration to admins and users SAAS TRIAL

                        else if (tenant.CreatedDateTime.Date.AddDays(21) == now)
                        {
                            action = Actions.SaasAdminUserAppsTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;
                        }

                        #endregion

                        #endregion

                        #region Trial warning letters

                        #region 5 days before SAAS TRIAL ends to admins

                        else if (duedate != DateTime.MaxValue && duedate.AddDays(-5) == now)
                        {
                            toadmins = true;
                            action = Actions.SaasAdminTrialWarningBefore5V10;
                            coupon = "PortalCreation10%";

                            if (string.IsNullOrEmpty(coupon))
                            {
                                try
                                {
                                    log.InfoFormat("start CreateCoupon to {0}", tenant.TenantAlias);

                                    coupon = SetupInfo.IsSecretEmail(userManager.GetUsers(tenant.OwnerId).Email)
                                                ? tenant.TenantAlias
                                                : CouponManager.CreateCoupon(tenantManager);

                                    log.InfoFormat("end CreateCoupon to {0} coupon = {1}", tenant.TenantAlias, coupon);
                                }
                                catch (AggregateException ae)
                                {
                                    foreach (var ex in ae.InnerExceptions)
                                        log.Error(ex);
                                }
                                catch (Exception ex)
                                {
                                    log.Error(ex);
                                }
                            }

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonUseDiscount;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                        }

                        #endregion

                        #region SAAS TRIAL expires today to admins

                        else if (duedate == now)
                        {
                            action = Actions.SaasAdminTrialWarningV10;
                            toadmins = true;
                        }

                        #endregion

                        #region 5 days after SAAS TRIAL expired to admins

                        else if (duedate != DateTime.MaxValue && duedate.AddDays(5) == now && tenant.VersionChanged <= tenant.CreatedDateTime)
                        {
                            action = Actions.SaasAdminTrialWarningAfter5V10;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSendRequest;
                            greenButtonUrl = "mailto:sales@onlyoffice.com";
                        }

                        #endregion

                        #region 30 days after SAAS TRIAL expired + only 1 user

                        else if (duedate != DateTime.MaxValue && duedate.AddDays(30) == now && userManager.GetUsers().Count() == 1)
                        {
                            action = Actions.SaasAdminTrialWarningAfter30V10;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonStartNow;
                            greenButtonUrl = "https://personal.onlyoffice.com";
                        }

                        #endregion

                        #region 6 months after SAAS TRIAL expired

                        else if (duedate != DateTime.MaxValue && duedate.AddMonths(6) == now)
                        {
                            action = Actions.SaasAdminTrialWarningAfterHalfYearV10;
                            toowner = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonLeaveFeedback;

                            var owner = userManager.GetUsers(tenant.OwnerId);
                            greenButtonUrl = SetupInfo.TeamlabSiteRedirect + "/remove-portal-feedback-form.aspx#" +
                                          Convert.ToBase64String(
                                              System.Text.Encoding.UTF8.GetBytes("{\"firstname\":\"" + owner.FirstName +
                                                                                 "\",\"lastname\":\"" + owner.LastName +
                                                                                 "\",\"alias\":\"" + tenant.TenantAlias +
                                                                                 "\",\"email\":\"" + owner.Email + "\"}"));
                        }
                        else if (duedate != DateTime.MaxValue && duedate.AddMonths(6).AddDays(7) <= now)
                        {
                            tenantManager.RemoveTenant(tenant.TenantId, true);

                            if (!string.IsNullOrEmpty(ApiSystemHelper.ApiCacheUrl))
                            {
                                ApiSystemHelper.RemoveTenantFromCache(tenant.TenantAlias, authContext.CurrentAccount.ID);
                            }
                        }

                        #endregion

                        #endregion
                    }
                    else if (tariff.State >= TariffState.Paid)
                    {
                        #region Payment warning letters

                        #region 5 days before SAAS PAID expired to admins

                        if (tariff.State == TariffState.Paid && duedate != DateTime.MaxValue && duedate.AddDays(-5) == now)
                        {
                            action = Actions.SaasAdminPaymentWarningBefore5V10;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonRenewNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                        }

                        #endregion

                        #region 1 day after SAAS PAID expired to admins

                        else if (tariff.State >= TariffState.Paid && duedate != DateTime.MaxValue && duedate.AddDays(1) == now)
                        {
                            action = Actions.SaasAdminPaymentWarningAfter1V10;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonRenewNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                        }

                        #endregion

                        #region 2 weeks after the payment of the monthly SAAS tariff

                        else if (tariff.State == TariffState.Paid && monthQuotasIds.Contains(tariff.QuotaId) && tariff.LicenseDate.AddDays(14) == now)
                        {
                            action = Actions.SaasAdminPaymentAfterMonthlySubscriptionsV10;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonBuyNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                        }

                        #endregion

                        #region 6 months after SAAS PAID expired

                        else if (tariff.State == TariffState.NotPaid && duedate != DateTime.MaxValue && duedate.AddMonths(6) == now)
                        {
                            action = Actions.SaasAdminTrialWarningAfterHalfYearV10;
                            toowner = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonLeaveFeedback;

                            var owner = userManager.GetUsers(tenant.OwnerId);
                            greenButtonUrl = SetupInfo.TeamlabSiteRedirect + "/remove-portal-feedback-form.aspx#" +
                                          Convert.ToBase64String(
                                              System.Text.Encoding.UTF8.GetBytes("{\"firstname\":\"" + owner.FirstName +
                                                                                 "\",\"lastname\":\"" + owner.LastName +
                                                                                 "\",\"alias\":\"" + tenant.TenantAlias +
                                                                                 "\",\"email\":\"" + owner.Email + "\"}"));
                        }
                        else if (tariff.State == TariffState.NotPaid && duedate != DateTime.MaxValue && duedate.AddMonths(6).AddDays(7) <= now)
                        {
                            tenantManager.RemoveTenant(tenant.TenantId, true);

                            if (!string.IsNullOrEmpty(ApiSystemHelper.ApiCacheUrl))
                            {
                                ApiSystemHelper.RemoveTenantFromCache(tenant.TenantAlias, authContext.CurrentAccount.ID);
                            }
                        }

                        #endregion

                        #endregion
                    }


                    if (action == null) continue;

                    var users = toowner
                                    ? new List<UserInfo> { userManager.GetUsers(tenant.OwnerId) }
                                    : studioNotifyHelper.GetRecipients(toadmins, tousers, false);


                    var analytics = StudioNotifyHelper.GetNotifyAnalytics(tenant.TenantId, action, toowner, toadmins, tousers, false);

                    foreach (var u in users.Where(u => paymentMessage || studioNotifyHelper.IsSubscribedToNotify(u, Actions.PeriodicNotify)))
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;
                        var rquota = tenantExtra.GetRightQuota() ?? TenantQuota.Default;

                        client.SendNoticeToAsync(
                            action,
                            new[] { studioNotifyHelper.ToRecipient(u.ID) },
                            new[] { senderName },
                            new TagValue(Tags.UserName, u.FirstName.HtmlEncode()),
                            new TagValue(Tags.PricingPage, CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx")),
                            new TagValue(Tags.ActiveUsers, userManager.GetUsers().Count()),
                            new TagValue(Tags.Price, rquota.Price),
                            new TagValue(Tags.PricePeriod, rquota.Year3 ? UserControlsCommonResource.TariffPerYear3 : rquota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth),
                            new TagValue(Tags.DueDate, duedate.ToLongDateString()),
                            new TagValue(Tags.DelayDueDate, (delayDuedate != DateTime.MaxValue ? delayDuedate : duedate).ToLongDateString()),
                            TagValues.BlueButton(blueButtonText, "http://www.onlyoffice.com/call-back-form.aspx"),
                            TagValues.GreenButton(greenButtonText, greenButtonUrl),
                            TagValues.TableTop(),
                            TagValues.TableItem(1, tableItemText1, tableItemUrl1, tableItemImg1, tableItemComment1, tableItemLearnMoreText1, tableItemLearnMoreUrl1),
                            TagValues.TableItem(2, tableItemText2, tableItemUrl2, tableItemImg2, tableItemComment2, tableItemLearnMoreText2, tableItemLearnMoreUrl2),
                            TagValues.TableItem(3, tableItemText3, tableItemUrl3, tableItemImg3, tableItemComment3, tableItemLearnMoreText3, tableItemLearnMoreUrl3),
                            TagValues.TableItem(4, tableItemText4, tableItemUrl4, tableItemImg4, tableItemComment4, tableItemLearnMoreText4, tableItemLearnMoreUrl4),
                            TagValues.TableItem(5, tableItemText5, tableItemUrl5, tableItemImg5, tableItemComment5, tableItemLearnMoreText5, tableItemLearnMoreUrl5),
                            TagValues.TableItem(6, tableItemText6, tableItemUrl6, tableItemImg6, tableItemComment6, tableItemLearnMoreText6, tableItemLearnMoreUrl6),
                            TagValues.TableItem(7, tableItemText7, tableItemUrl7, tableItemImg7, tableItemComment7, tableItemLearnMoreText7, tableItemLearnMoreUrl7),
                            TagValues.TableBottom(),
                            new TagValue(CommonTags.Footer, u.IsAdmin(userManager) ? "common" : "social"),
                            new TagValue(CommonTags.Analytics, analytics),
                            new TagValue(Tags.Coupon, coupon));
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendSaasTariffLetters");
        }

        public static void SendEnterpriseLetters(INotifyClient client, string senderName, DateTime scheduleDate, IServiceProvider serviceProvider)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var now = scheduleDate.Date;
            const string dbid = "webstudio";

            log.Info("Start SendTariffEnterpriseLetters");

            var activeTenants = new List<Tenant>();

            using (var scope = serviceProvider.CreateScope())
            {
                var tenantManager = scope.ServiceProvider.GetService<TenantManager>();

                activeTenants = tenantManager.GetTenants().ToList();

                if (activeTenants.Count <= 0)
                {
                    log.Info("End SendTariffEnterpriseLetters");
                    return;
                }
            }

            foreach (var tenant in activeTenants)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var tenantManager = scope.ServiceProvider.GetService<TenantManager>();
                    var defaultRebranding = scope.ServiceProvider.GetService<MailWhiteLabelSettings>().Instance.IsDefault;

                    tenantManager.SetCurrentTenant(tenant.TenantId);

                    var userManager = scope.ServiceProvider.GetService<UserManager>();
                    var studioNotifyHelper = scope.ServiceProvider.GetService<StudioNotifyHelper>();
                    var tariff = CoreContext.PaymentManager.GetTariff(tenant.TenantId);
                    var quota = tenantManager.GetTenantQuota(tenant.TenantId);
                    var tenantExtra = scope.ServiceProvider.GetService<TenantExtra>();

                    var duedate = tariff.DueDate.Date;
                    var delayDuedate = tariff.DelayDueDate.Date;

                    INotifyAction action = null;
                    var paymentMessage = true;

                    var toadmins = false;
                    var tousers = false;

                    Func<string> greenButtonText = () => string.Empty;
                    string blueButtonText() => WebstudioNotifyPatternResource.ButtonRequestCallButton;
                    var greenButtonUrl = string.Empty;

                    Func<string> tableItemText1 = () => string.Empty;
                    Func<string> tableItemText2 = () => string.Empty;
                    Func<string> tableItemText3 = () => string.Empty;
                    Func<string> tableItemText4 = () => string.Empty;
                    Func<string> tableItemText5 = () => string.Empty;
                    Func<string> tableItemText6 = () => string.Empty;
                    Func<string> tableItemText7 = () => string.Empty;

                    var tableItemUrl1 = string.Empty;
                    var tableItemUrl2 = string.Empty;
                    var tableItemUrl3 = string.Empty;
                    var tableItemUrl4 = string.Empty;
                    var tableItemUrl5 = string.Empty;
                    var tableItemUrl6 = string.Empty;
                    var tableItemUrl7 = string.Empty;

                    var tableItemImg1 = string.Empty;
                    var tableItemImg2 = string.Empty;
                    var tableItemImg3 = string.Empty;
                    var tableItemImg4 = string.Empty;
                    var tableItemImg5 = string.Empty;
                    var tableItemImg6 = string.Empty;
                    var tableItemImg7 = string.Empty;

                    Func<string> tableItemComment1 = () => string.Empty;
                    Func<string> tableItemComment2 = () => string.Empty;
                    Func<string> tableItemComment3 = () => string.Empty;
                    Func<string> tableItemComment4 = () => string.Empty;
                    Func<string> tableItemComment5 = () => string.Empty;
                    Func<string> tableItemComment6 = () => string.Empty;
                    Func<string> tableItemComment7 = () => string.Empty;

                    Func<string> tableItemLearnMoreText1 = () => string.Empty;
                    string tableItemLearnMoreText2() => string.Empty;
                    string tableItemLearnMoreText3() => string.Empty;
                    string tableItemLearnMoreText4() => string.Empty;
                    string tableItemLearnMoreText5() => string.Empty;
                    string tableItemLearnMoreText6() => string.Empty;
                    string tableItemLearnMoreText7() => string.Empty;

                    var tableItemLearnMoreUrl1 = string.Empty;
                    var tableItemLearnMoreUrl2 = string.Empty;
                    var tableItemLearnMoreUrl3 = string.Empty;
                    var tableItemLearnMoreUrl4 = string.Empty;
                    var tableItemLearnMoreUrl5 = string.Empty;
                    var tableItemLearnMoreUrl6 = string.Empty;
                    var tableItemLearnMoreUrl7 = string.Empty;


                    if (quota.Trial && defaultRebranding)
                    {
                        #region After registration letters

                        #region 1 day after registration to admins ENTERPRISE TRIAL + defaultRebranding

                        if (tenant.CreatedDateTime.Date.AddDays(1) == now)
                        {
                            action = Actions.EnterpriseAdminCustomizePortalV10;
                            paymentMessage = false;
                            toadmins = true;

                            tableItemImg1 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-brand-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_brand_hdr;
                            tableItemComment1 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_brand;

                            tableItemImg2 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-regional-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_regional_hdr;
                            tableItemComment2 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_regional;

                            tableItemImg3 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-customize-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_customize_hdr;
                            tableItemComment3 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_customize;

                            tableItemImg4 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-modules-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_modules_hdr;
                            tableItemComment4 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_modules;

                            tableItemImg5 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-3rdparty-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_3rdparty_hdr;
                            tableItemComment5 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_3rdparty;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonConfigureRightNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetAdministration(ManagementType.General));
                        }

                        #endregion

                        #region 4 days after registration to admins ENTERPRISE TRIAL + only 1 user + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(4) == now && userManager.GetUsers().Count() == 1)
                        {
                            action = Actions.EnterpriseAdminInviteTeammatesV10;
                            paymentMessage = false;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonInviteRightNow;
                            greenButtonUrl = string.Format("{0}/products/people/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 5 days after registration to admins ENTERPRISE TRAIL + without activity in 1 or more days + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(5) == now)
                        {
                            List<DateTime> datesWithActivity;

                            var query = new SqlQuery("feed_aggregate")
                                .Select(new SqlExp("cast(created_date as date) as short_date"))
                                .Where("tenant", tenantManager.GetCurrentTenant().TenantId)
                                .Where(Exp.Le("created_date", now.AddDays(-1)))
                                .GroupBy("short_date");

                            using (var db = new DbManager(dbid))
                            {
                                datesWithActivity = db
                                    .ExecuteList(query)
                                    .ConvertAll(r => Convert.ToDateTime(r[0]));
                            }

                            if (datesWithActivity.Count < 5)
                            {
                                action = Actions.EnterpriseAdminWithoutActivityV10;
                                paymentMessage = false;
                                toadmins = true;
                            }
                        }

                        #endregion

                        #region 7 days after registration to admins and users ENTERPRISE TRIAL + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(7) == now)
                        {
                            action = Actions.EnterpriseAdminUserDocsTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-formatting-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_formatting_hdr;
                            tableItemComment1 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_formatting;
                            tableItemLearnMoreUrl1 = studioNotifyHelper.Helplink + "onlyoffice-editors/index.aspx";
                            tableItemLearnMoreText1 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg2 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-share-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_share_hdr;
                            tableItemComment2 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_share;

                            tableItemImg3 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-coediting-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_coediting_hdr;
                            tableItemComment3 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_coediting;

                            tableItemImg4 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-review-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_review_hdr;
                            tableItemComment4 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_review;

                            tableItemImg5 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-3rdparty-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_3rdparty_hdr;
                            tableItemComment5 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_3rdparty;

                            tableItemImg6 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-attach-100.png";
                            tableItemText6 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_attach_hdr;
                            tableItemComment6 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_attach;

                            tableItemImg7 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-documents-apps-100.png";
                            tableItemText7 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_apps_hdr;
                            tableItemComment7 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_apps;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;
                            greenButtonUrl = string.Format("{0}/products/files/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 21 days after registration to admins and users ENTERPRISE TRIAL + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(21) == now)
                        {
                            action = Actions.EnterpriseAdminUserAppsTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;
                        }

                        #endregion

                        #endregion

                        #region Trial warning letters

                        #region 7 days before ENTERPRISE TRIAL ends to admins + defaultRebranding

                        else if (duedate != DateTime.MaxValue && duedate.AddDays(-7) == now)
                        {
                            action = Actions.EnterpriseAdminTrialWarningBefore7V10;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = "http://www.onlyoffice.com/enterprise-edition.aspx";
                        }

                        #endregion

                        #region ENTERPRISE TRIAL expires today to admins + defaultRebranding

                        else if (duedate == now)
                        {
                            action = Actions.EnterpriseAdminTrialWarningV10;
                            toadmins = true;
                        }

                        #endregion

                        #endregion
                    }
                    else if (quota.Trial && !defaultRebranding)
                    {
                        #region After registration letters

                        #region 1 day after registration to admins ENTERPRISE TRIAL + !defaultRebranding

                        if (tenant.CreatedDateTime.Date.AddDays(1) == now)
                        {
                            action = Actions.EnterpriseWhitelabelAdminCustomizePortalV10;
                            paymentMessage = false;
                            toadmins = true;

                            tableItemImg1 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-brand-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_brand_hdr;
                            tableItemComment1 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_brand;

                            tableItemImg2 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-regional-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_regional_hdr;
                            tableItemComment2 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_regional;

                            tableItemImg3 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-customize-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_customize_hdr;
                            tableItemComment3 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_customize;

                            tableItemImg4 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-modules-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_modules_hdr;
                            tableItemComment4 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_modules;

                            if (!CoreContext.Configuration.CustomMode)
                            {
                                tableItemImg5 = "https://static.onlyoffice.com/media/newsletters/images-v10/tips-customize-3rdparty-100.png";
                                tableItemText5 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_3rdparty_hdr;
                                tableItemComment5 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_3rdparty;
                            }

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonConfigureRightNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetAdministration(ManagementType.General));
                        }

                        #endregion

                        #endregion
                    }
                    else if (tariff.State == TariffState.Paid)
                    {
                        #region Payment warning letters

                        #region 7 days before ENTERPRISE PAID expired to admins

                        if (duedate != DateTime.MaxValue && duedate.AddDays(-7) == now)
                        {
                            action = defaultRebranding
                                         ? Actions.EnterpriseAdminPaymentWarningBefore7V10
                                         : Actions.EnterpriseWhitelabelAdminPaymentWarningBefore7V10;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                        }

                        #endregion

                        #region ENTERPRISE PAID expires today to admins

                        else if (duedate == now)
                        {
                            action = defaultRebranding
                                         ? Actions.EnterpriseAdminPaymentWarningV10
                                         : Actions.EnterpriseWhitelabelAdminPaymentWarningV10;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                        }

                        #endregion

                        #endregion
                    }


                    if (action == null) continue;

                    var users = studioNotifyHelper.GetRecipients(toadmins, tousers, false);

                    foreach (var u in users.Where(u => paymentMessage || studioNotifyHelper.IsSubscribedToNotify(u, Actions.PeriodicNotify)))
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        var rquota = tenantExtra.GetRightQuota() ?? TenantQuota.Default;

                        client.SendNoticeToAsync(
                            action,
                            new[] { studioNotifyHelper.ToRecipient(u.ID) },
                            new[] { senderName },
                            new TagValue(Tags.UserName, u.FirstName.HtmlEncode()),
                            new TagValue(Tags.PricingPage, CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx")),
                            new TagValue(Tags.ActiveUsers, userManager.GetUsers().Count()),
                            new TagValue(Tags.Price, rquota.Price),
                            new TagValue(Tags.PricePeriod, rquota.Year3 ? UserControlsCommonResource.TariffPerYear3 : rquota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth),
                            new TagValue(Tags.DueDate, duedate.ToLongDateString()),
                            new TagValue(Tags.DelayDueDate, (delayDuedate != DateTime.MaxValue ? delayDuedate : duedate).ToLongDateString()),
                            TagValues.BlueButton(blueButtonText, "http://www.onlyoffice.com/call-back-form.aspx"),
                            TagValues.GreenButton(greenButtonText, greenButtonUrl),
                            TagValues.TableTop(),
                            TagValues.TableItem(1, tableItemText1, tableItemUrl1, tableItemImg1, tableItemComment1, tableItemLearnMoreText1, tableItemLearnMoreUrl1),
                            TagValues.TableItem(2, tableItemText2, tableItemUrl2, tableItemImg2, tableItemComment2, tableItemLearnMoreText2, tableItemLearnMoreUrl2),
                            TagValues.TableItem(3, tableItemText3, tableItemUrl3, tableItemImg3, tableItemComment3, tableItemLearnMoreText3, tableItemLearnMoreUrl3),
                            TagValues.TableItem(4, tableItemText4, tableItemUrl4, tableItemImg4, tableItemComment4, tableItemLearnMoreText4, tableItemLearnMoreUrl4),
                            TagValues.TableItem(5, tableItemText5, tableItemUrl5, tableItemImg5, tableItemComment5, tableItemLearnMoreText5, tableItemLearnMoreUrl5),
                            TagValues.TableItem(6, tableItemText6, tableItemUrl6, tableItemImg6, tableItemComment6, tableItemLearnMoreText6, tableItemLearnMoreUrl6),
                            TagValues.TableItem(7, tableItemText7, tableItemUrl7, tableItemImg7, tableItemComment7, tableItemLearnMoreText7, tableItemLearnMoreUrl7),
                            TagValues.TableBottom());
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendTariffEnterpriseLetters");
        }

        public static void SendOpensourceLetters(INotifyClient client, string senderName, DateTime scheduleDate, IServiceProvider serviceProvider)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var now = scheduleDate.Date;

            log.Info("Start SendOpensourceTariffLetters");

            var activeTenants = new List<Tenant>();

            using (var scope = serviceProvider.CreateScope())
            {
                var tenantManager = scope.ServiceProvider.GetService<TenantManager>();

                activeTenants = tenantManager.GetTenants().ToList();

                if (activeTenants.Count <= 0)
                {
                    log.Info("End SendOpensourceTariffLetters");
                    return;
                }
            }

            foreach (var tenant in activeTenants)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var tenantManager = scope.ServiceProvider.GetService<TenantManager>();

                    tenantManager.SetCurrentTenant(tenant.TenantId);

                    var userManager = scope.ServiceProvider.GetService<UserManager>();
                    var studioNotifyHelper = scope.ServiceProvider.GetService<StudioNotifyHelper>();

                    INotifyAction action = null;

                    Func<string> greenButtonText = () => string.Empty;
                    var greenButtonUrl = string.Empty;

                    string tableItemText1() => string.Empty;
                    string tableItemText2() => string.Empty;
                    string tableItemText3() => string.Empty;
                    string tableItemText4() => string.Empty;
                    string tableItemText5() => string.Empty;
                    string tableItemText6() => string.Empty;
                    string tableItemText7() => string.Empty;

                    var tableItemUrl1 = string.Empty;
                    var tableItemUrl2 = string.Empty;
                    var tableItemUrl3 = string.Empty;
                    var tableItemUrl4 = string.Empty;
                    var tableItemUrl5 = string.Empty;
                    var tableItemUrl6 = string.Empty;
                    var tableItemUrl7 = string.Empty;

                    var tableItemImg1 = string.Empty;
                    var tableItemImg2 = string.Empty;
                    var tableItemImg3 = string.Empty;
                    var tableItemImg4 = string.Empty;
                    var tableItemImg5 = string.Empty;
                    var tableItemImg6 = string.Empty;
                    var tableItemImg7 = string.Empty;

                    Func<string> tableItemComment1 = () => string.Empty;
                    Func<string> tableItemComment2 = () => string.Empty;
                    Func<string> tableItemComment3 = () => string.Empty;
                    Func<string> tableItemComment4 = () => string.Empty;
                    Func<string> tableItemComment5 = () => string.Empty;
                    Func<string> tableItemComment6 = () => string.Empty;
                    Func<string> tableItemComment7 = () => string.Empty;

                    Func<string> tableItemLearnMoreText1 = () => string.Empty;
                    Func<string> tableItemLearnMoreText2 = () => string.Empty;
                    Func<string> tableItemLearnMoreText3 = () => string.Empty;
                    Func<string> tableItemLearnMoreText4 = () => string.Empty;
                    Func<string> tableItemLearnMoreText5 = () => string.Empty;
                    Func<string> tableItemLearnMoreText6 = () => string.Empty;
                    Func<string> tableItemLearnMoreText7 = () => string.Empty;

                    var tableItemLearnMoreUrl1 = string.Empty;
                    var tableItemLearnMoreUrl2 = string.Empty;
                    var tableItemLearnMoreUrl3 = string.Empty;
                    var tableItemLearnMoreUrl4 = string.Empty;
                    var tableItemLearnMoreUrl5 = string.Empty;
                    var tableItemLearnMoreUrl6 = string.Empty;
                    var tableItemLearnMoreUrl7 = string.Empty;


                    #region After registration letters

                    #region 7 days after registration to admins

                    if (tenant.CreatedDateTime.Date.AddDays(7) == now)
                    {
                        action = Actions.OpensourceAdminSecurityTips;

                        greenButtonText = () => WebstudioNotifyPatternResource.ButtonStartFreeTrial;
                        greenButtonUrl = "https://www.onlyoffice.com/enterprise-edition-free.aspx";
                    }

                    #endregion

                    #region 3 weeks after registration to admins

                    else if (tenant.CreatedDateTime.Date.AddDays(21) == now)
                    {
                        action = Actions.OpensourceAdminDocsTips;

                        tableItemImg1 = "https://static.onlyoffice.com/media/newsletters/images/tips-documents-01-100.png";
                        tableItemComment1 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips1;
                        tableItemLearnMoreUrl1 = studioNotifyHelper.Helplink + "/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/CollaborativeEditing.aspx";
                        tableItemLearnMoreText1 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                        tableItemImg2 = "https://static.onlyoffice.com/media/newsletters/images/tips-documents-02-100.png";
                        tableItemComment2 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips2;
                        tableItemLearnMoreUrl2 = studioNotifyHelper.Helplink + "/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/UsageInstructions/ViewDocInfo.aspx";
                        tableItemLearnMoreText2 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                        tableItemImg3 = "https://static.onlyoffice.com/media/newsletters/images/tips-documents-07-100.png";
                        tableItemComment3 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips3;
                        tableItemLearnMoreUrl3 = studioNotifyHelper.Helplink + "/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/Review.aspx";
                        tableItemLearnMoreText3 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                        tableItemImg4 = "https://static.onlyoffice.com/media/newsletters/images/tips-documents-03-100.png";
                        tableItemComment4 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips4;
                        tableItemLearnMoreUrl4 = studioNotifyHelper.Helplink + "/gettingstarted/documents.aspx#SharingDocuments_block";
                        tableItemLearnMoreText4 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                        tableItemImg5 = "https://static.onlyoffice.com/media/newsletters/images/tips-documents-04-100.png";
                        tableItemComment5 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips5;
                        tableItemLearnMoreUrl5 = studioNotifyHelper.Helplink + "/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/UsageInstructions/UseMailMerge.aspx";
                        tableItemLearnMoreText5 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                        tableItemImg6 = "https://static.onlyoffice.com/media/newsletters/images/tips-documents-08-100.png";
                        tableItemComment6 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips6;
                        tableItemLearnMoreUrl6 = "http://www.onlyoffice.com/desktop.aspx";
                        tableItemLearnMoreText6 = () => WebstudioNotifyPatternResource.ButtonDownloadNow;

                        tableItemImg7 = "https://static.onlyoffice.com/media/newsletters/images/tips-documents-05-100.png";
                        tableItemComment7 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips7;
                        tableItemLearnMoreUrl7 = "https://itunes.apple.com/us/app/onlyoffice-documents/id944896972";
                        tableItemLearnMoreText7 = () => WebstudioNotifyPatternResource.ButtonGoToAppStore;
                    }

                    #endregion

                    #endregion


                    if (action == null) continue;

                    var users = studioNotifyHelper.GetRecipients(true, false, false);

                    foreach (var u in users.Where(u => studioNotifyHelper.IsSubscribedToNotify(u, Actions.PeriodicNotify)))
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        client.SendNoticeToAsync(
                            action,
                            new[] { studioNotifyHelper.ToRecipient(u.ID) },
                            new[] { senderName },
                            new TagValue(Tags.UserName, u.DisplayUserName(userManager)),
                            TagValues.GreenButton(greenButtonText, greenButtonUrl),
                            TagValues.TableTop(),
                            TagValues.TableItem(1, tableItemText1, tableItemUrl1, tableItemImg1, tableItemComment1, tableItemLearnMoreText1, tableItemLearnMoreUrl1),
                            TagValues.TableItem(2, tableItemText2, tableItemUrl2, tableItemImg2, tableItemComment2, tableItemLearnMoreText2, tableItemLearnMoreUrl2),
                            TagValues.TableItem(3, tableItemText3, tableItemUrl3, tableItemImg3, tableItemComment3, tableItemLearnMoreText3, tableItemLearnMoreUrl3),
                            TagValues.TableItem(4, tableItemText4, tableItemUrl4, tableItemImg4, tableItemComment4, tableItemLearnMoreText4, tableItemLearnMoreUrl4),
                            TagValues.TableItem(5, tableItemText5, tableItemUrl5, tableItemImg5, tableItemComment5, tableItemLearnMoreText5, tableItemLearnMoreUrl5),
                            TagValues.TableItem(6, tableItemText6, tableItemUrl6, tableItemImg6, tableItemComment6, tableItemLearnMoreText6, tableItemLearnMoreUrl6),
                            TagValues.TableItem(7, tableItemText7, tableItemUrl7, tableItemImg7, tableItemComment7, tableItemLearnMoreText7, tableItemLearnMoreUrl7),
                            TagValues.TableBottom(),
                            new TagValue(CommonTags.Footer, "opensource"));
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendOpensourceTariffLetters");
        }

        public static void SendPersonalLetters(INotifyClient client, string senderName, DateTime scheduleDate, IServiceProvider serviceProvider)
        {
            var log = LogManager.GetLogger("ASC.Notify");

            log.Info("Start SendLettersPersonal...");


            var activeTenants = new List<Tenant>();

            using (var scope = serviceProvider.CreateScope())
            {
                var tenantManager = scope.ServiceProvider.GetService<TenantManager>();

                activeTenants = tenantManager.GetTenants().ToList();
            }

            foreach (var tenant in activeTenants)
            {
                try
                {
                    Func<string> greenButtonText = () => string.Empty;
                    var greenButtonUrl = string.Empty;

                    var sendCount = 0;

                    using var scope = serviceProvider.CreateScope();
                    var tenantManager = scope.ServiceProvider.GetService<TenantManager>();

                    tenantManager.SetCurrentTenant(tenant.TenantId);

                    var userManager = scope.ServiceProvider.GetService<UserManager>();
                    var studioNotifyHelper = scope.ServiceProvider.GetService<StudioNotifyHelper>();
                    var SecurityContext = scope.ServiceProvider.GetService<SecurityContext>();
                    var Authentication = scope.ServiceProvider.GetService<AuthManager>();

                    log.InfoFormat("Current tenant: {0}", tenant.TenantId);

                    var users = userManager.GetUsers(EmployeeStatus.Active);

                    foreach (var user in users.Where(u => studioNotifyHelper.IsSubscribedToNotify(u, Actions.PeriodicNotify)))
                    {
                        INotifyAction action;

                        SecurityContext.AuthenticateMe(Authentication.GetAccountByID(tenant.TenantId, user.ID));

                        var culture = tenant.GetCulture();
                        if (!string.IsNullOrEmpty(user.CultureName))
                        {
                            try
                            {
                                culture = user.GetCulture();
                            }
                            catch (CultureNotFoundException exception)
                            {

                                log.Error(exception);
                            }
                        }

                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        var dayAfterRegister = (int)scheduleDate.Date.Subtract(user.CreateDate.Date).TotalDays;

                        if (CoreContext.Configuration.CustomMode)
                        {
                            switch (dayAfterRegister)
                            {
                                case 7:
                                    action = Actions.PersonalCustomModeAfterRegistration7;
                                    break;
                                default:
                                    continue;
                            }
                        }
                        else
                        {

                            switch (dayAfterRegister)
                            {
                                case 7:
                                    action = Actions.PersonalAfterRegistration7;
                                    break;
                                case 14:
                                    action = Actions.PersonalAfterRegistration14;
                                    break;
                                case 21:
                                    action = Actions.PersonalAfterRegistration21;
                                    break;
                                case 28:
                                    action = Actions.PersonalAfterRegistration28;
                                    greenButtonText = () => WebstudioNotifyPatternResource.ButtonStartFreeTrial;
                                    greenButtonUrl = "https://www.onlyoffice.com/enterprise-edition-free.aspx";
                                    break;
                                default:
                                    continue;
                            }
                        }

                        if (action == null) continue;

                        log.InfoFormat(@"Send letter personal '{1}'  to {0} culture {2}. tenant id: {3} user culture {4} create on {5} now date {6}",
                              user.Email, action.ID, culture, tenant.TenantId, user.GetCulture(), user.CreateDate, scheduleDate.Date);

                        sendCount++;

                        client.SendNoticeToAsync(
                          action,
                          null,
                          studioNotifyHelper.RecipientFromEmail(new[] { user.Email.ToLower() }, true),
                          new[] { senderName },
                          TagValues.PersonalHeaderStart(),
                          TagValues.PersonalHeaderEnd(),
                          TagValues.GreenButton(greenButtonText, greenButtonUrl),
                          new TagValue(CommonTags.Footer, CoreContext.Configuration.CustomMode ? "personalCustomMode" : "personal"));
                    }

                    log.InfoFormat("Total send count: {0}", sendCount);
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendLettersPersonal.");
        }

        public static bool ChangeSubscription(Guid userId, StudioNotifyHelper studioNotifyHelper)
        {
            var recipient = studioNotifyHelper.ToRecipient(userId);

            var isSubscribe = studioNotifyHelper.IsSubscribedToNotify(recipient, Actions.PeriodicNotify);

            studioNotifyHelper.SubscribeToNotify(recipient, Actions.PeriodicNotify, !isSubscribe);

            return !isSubscribe;
        }
    }
}