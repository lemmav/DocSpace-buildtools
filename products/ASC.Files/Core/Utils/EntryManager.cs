// (c) Copyright Ascensio System SIA 2010-2022
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

namespace ASC.Web.Files.Utils;

[Scope]
public class LockerManager
{
    private readonly AuthContext _authContext;
    private readonly IDaoFactory _daoFactory;
    private readonly ThirdPartySelector _thirdPartySelector;
    public LockerManager(AuthContext authContext, IDaoFactory daoFactory, ThirdPartySelector thirdPartySelector)
    {
        _authContext = authContext;
        _daoFactory = daoFactory;
        _thirdPartySelector = thirdPartySelector;
    }

    public bool FileLockedForMe<T>(T fileId, Guid userId = default)
    {
        var app = _thirdPartySelector.GetAppByFileId(fileId.ToString());
        if (app != null)
        {
            return false;
        }

        userId = userId == default ? _authContext.CurrentAccount.ID : userId;
        var tagDao = _daoFactory.GetTagDao<T>();
        var lockedBy = FileLockedBy(fileId, tagDao);

        return lockedBy != Guid.Empty && lockedBy != userId;
    }

    public async Task<bool> FileLockedForMeAsync<T>(T fileId, Guid userId = default)
    {
        var app = _thirdPartySelector.GetAppByFileId(fileId.ToString());
        if (app != null)
        {
            return false;
        }

        userId = userId == default ? _authContext.CurrentAccount.ID : userId;
        var tagDao = _daoFactory.GetTagDao<T>();
        var lockedBy = await FileLockedByAsync(fileId, tagDao);

        return lockedBy != Guid.Empty && lockedBy != userId;
    }

    public Guid FileLockedBy<T>(T fileId, ITagDao<T> tagDao)
    {
        var tagLock = tagDao.GetTagsAsync(fileId, FileEntryType.File, TagType.Locked).ToListAsync().Result.FirstOrDefault();

        return tagLock != null ? tagLock.Owner : Guid.Empty;
    }

    public async Task<Guid> FileLockedByAsync<T>(T fileId, ITagDao<T> tagDao)
    {
        var tags = tagDao.GetTagsAsync(fileId, FileEntryType.File, TagType.Locked);
        var tagLock = await tags.FirstOrDefaultAsync();

        return tagLock != null ? tagLock.Owner : Guid.Empty;
    }
}

[Scope]
public class BreadCrumbsManager
{
    private readonly IDaoFactory _daoFactory;
    private readonly FileSecurity _fileSecurity;
    private readonly GlobalFolderHelper _globalFolderHelper;
    private readonly AuthContext _authContext;

    public BreadCrumbsManager(
        IDaoFactory daoFactory,
        FileSecurity fileSecurity,
        GlobalFolderHelper globalFolderHelper,
        AuthContext authContext)
    {
        _daoFactory = daoFactory;
        _fileSecurity = fileSecurity;
        _globalFolderHelper = globalFolderHelper;
        _authContext = authContext;
    }

    public Task<List<FileEntry>> GetBreadCrumbsAsync<T>(T folderId)
    {
        var folderDao = _daoFactory.GetFolderDao<T>();

        return GetBreadCrumbsAsync(folderId, folderDao);
    }

    public async Task<List<FileEntry>> GetBreadCrumbsAsync<T>(T folderId, IFolderDao<T> folderDao)
    {
        if (folderId == null)
        {
            return new List<FileEntry>();
        }

        var tmpBreadCrumbs = await _fileSecurity.FilterReadAsync(await folderDao.GetParentFoldersAsync(folderId));
        var breadCrumbs = tmpBreadCrumbs.Cast<FileEntry>().ToList();

        var firstVisible = breadCrumbs.ElementAtOrDefault(0) as Folder<T>;

        var rootId = 0;
        if (firstVisible == null)
        {
            rootId = await _globalFolderHelper.FolderShareAsync;
        }
        else
        {
            switch (firstVisible.FolderType)
            {
                case FolderType.DEFAULT:
                    if (!firstVisible.ProviderEntry)
                    {
                        rootId = await _globalFolderHelper.FolderShareAsync;
                    }
                    else
                    {
                        switch (firstVisible.RootFolderType)
                        {
                            case FolderType.USER:
                                rootId = _authContext.CurrentAccount.ID == firstVisible.RootCreateBy
                                    ? _globalFolderHelper.FolderMy
                                    : await _globalFolderHelper.FolderShareAsync;
                                break;
                            case FolderType.COMMON:
                                rootId = await _globalFolderHelper.FolderCommonAsync;
                                break;
                        }
                    }
                    break;

                case FolderType.BUNCH:
                    rootId = await _globalFolderHelper.FolderProjectsAsync;
                    break;
            }
        }

        var folderDaoInt = _daoFactory.GetFolderDao<int>();

        if (rootId != 0)
        {
            breadCrumbs.Insert(0, await folderDaoInt.GetFolderAsync(rootId));
        }

        return breadCrumbs;
    }
}

[Scope]
public class EntryStatusManager
{
    private readonly IDaoFactory _daoFactory;
    private readonly AuthContext _authContext;
    private readonly Global _global;

    public EntryStatusManager(IDaoFactory daoFactory, AuthContext authContext, Global global)
    {
        _daoFactory = daoFactory;
        _authContext = authContext;
        _global = global;
    }

    public async Task SetFileStatusAsync<T>(File<T> file)
    {
        if (file == null || file.Id == null)
        {
            return;
        }

        await SetFileStatusAsync(new List<File<T>>(1) { file });
    }

    public async Task SetFileStatusAsync(IEnumerable<FileEntry> files)
    {
        await SetFileStatusAsync(files.OfType<File<int>>().Where(r => r.Id != 0).ToList());
        await SetFileStatusAsync(files.OfType<File<string>>().Where(r => !string.IsNullOrEmpty(r.Id)).ToList());
    }

    public async Task SetFileStatusAsyncEnumerable(IAsyncEnumerable<FileEntry> asyncEnumerableFiles)
    {
        var files = await asyncEnumerableFiles.ToListAsync();
        await SetFileStatusAsync(files.OfType<File<int>>().Where(r => r.Id != 0));
        await SetFileStatusAsync(files.OfType<File<string>>().Where(r => !string.IsNullOrEmpty(r.Id)));
    }

    public async Task SetFileStatusAsync<T>(IEnumerable<File<T>> files)
    {
        var tagDao = _daoFactory.GetTagDao<T>();

        var tags = await tagDao.GetTagsAsync(_authContext.CurrentAccount.ID, new[] { TagType.Favorite, TagType.Template, TagType.Locked }, files);
        var tagsNew = await tagDao.GetNewTagsAsync(_authContext.CurrentAccount.ID, files).ToListAsync();

        foreach (var file in files)
        {
            foreach (var t in tags)
            {
                if (!t.Key.Equals(file.Id))
                {
                    continue;
                }

                if (t.Value.Any(r => r.Type == TagType.Favorite))
                {
                    file.IsFavorite = true;
                }

                if (t.Value.Any(r => r.Type == TagType.Template))
                {
                    file.IsTemplate = true;
                }

                var lockedTag = t.Value.FirstOrDefault(r => r.Type == TagType.Locked);
                if (lockedTag != null)
                {
                    var lockedBy = lockedTag.Owner;
                    file.Locked = lockedBy != Guid.Empty;
                    file.LockedBy = lockedBy != Guid.Empty && lockedBy != _authContext.CurrentAccount.ID
                        ? _global.GetUserName(lockedBy)
                        : null;

                    continue;
                }
            }

            if (tagsNew.Any(r => r.EntryId.Equals(file.Id)))
            {
                file.IsNew = true;
            }
        }
    }

    public async Task SetIsFavoriteFolderAsync<T>(Folder<T> folder)
    {
        if (folder == null || folder.Id == null)
        {
            return;
        }

        await SetIsFavoriteFoldersAsync(new List<Folder<T>>(1) { folder });
    }

    public async Task SetIsFavoriteFoldersAsync(IEnumerable<FileEntry> files)
    {
        await SetIsFavoriteFoldersAsync(files.OfType<Folder<int>>().Where(r => r.Id != 0).ToList());
        await SetIsFavoriteFoldersAsync(files.OfType<Folder<string>>().Where(r => !string.IsNullOrEmpty(r.Id)).ToList());
    }

    public async Task SetIsFavoriteFoldersAsync<T>(IEnumerable<Folder<T>> folders)
    {
        var tagDao = _daoFactory.GetTagDao<T>();

        var tagsFavorite = await tagDao.GetTagsAsync(_authContext.CurrentAccount.ID, TagType.Favorite, folders).ToListAsync();

        foreach (var folder in folders)
        {
            if (tagsFavorite.Any(r => r.EntryId.Equals(folder.Id)))
            {
                folder.IsFavorite = true;
            }
        }
    }

    public async Task SetFileStatusAsyncEnumerable<T>(IAsyncEnumerable<File<T>> files)
    {
        var tagDao = _daoFactory.GetTagDao<T>();

        var tags = await tagDao.GetTagsAsync(_authContext.CurrentAccount.ID, new[] { TagType.Favorite, TagType.Template, TagType.Locked }, files);
        var tagsNew = tagDao.GetNewTagsAsync(_authContext.CurrentAccount.ID, files);

        await foreach (var file in files)
        {
            foreach (var t in tags)
            {
                if (!t.Key.Equals(file.Id))
                {
                    continue;
                }


                if (t.Value.Any(r => r.Type == TagType.Favorite))
                {
                    file.IsFavorite = true;
                }

                if (t.Value.Any(r => r.Type == TagType.Template))
                {
                    file.IsTemplate = true;
                }

                var lockedTag = t.Value.FirstOrDefault(r => r.Type == TagType.Locked);
                if (lockedTag != null)
                {
                    var lockedBy = lockedTag.Owner;
                    file.Locked = lockedBy != Guid.Empty;
                    file.LockedBy = lockedBy != Guid.Empty && lockedBy != _authContext.CurrentAccount.ID
                        ? _global.GetUserName(lockedBy)
                        : null;

                    continue;
                }
            }

            if (await tagsNew.AnyAsync(r => r.EntryId.Equals(file.Id)))
            {
                file.IsNew = true;
            }
        }
    }
}

[Scope]
public class EntryManager
{
    private const string _updateList = "filesUpdateList";
    private readonly ThirdPartySelector _thirdPartySelector;
    private readonly ThumbnailSettings _thumbnailSettings;

    private readonly ICache _cache;
    private readonly FileTrackerHelper _fileTracker;
    private readonly EntryStatusManager _entryStatusManager;
    private readonly IDaoFactory _daoFactory;
    private readonly FileSecurity _fileSecurity;
    private readonly GlobalFolderHelper _globalFolderHelper;
    private readonly PathProvider _pathProvider;
    private readonly AuthContext _authContext;
    private readonly FileMarker _fileMarker;
    private readonly FileUtility _fileUtility;
    private readonly GlobalStore _globalStore;
    private readonly CoreBaseSettings _coreBaseSettings;
    private readonly FilesSettingsHelper _filesSettingsHelper;
    private readonly UserManager _userManager;
    private readonly FileShareLink _fileShareLink;
    private readonly DocumentServiceHelper _documentServiceHelper;
    private readonly ThirdpartyConfiguration _thirdpartyConfiguration;
    private readonly DocumentServiceConnector _documentServiceConnector;
    private readonly LockerManager _lockerManager;
    private readonly BreadCrumbsManager _breadCrumbsManager;
    private readonly SettingsManager _settingsManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EntryManager> _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly FilesMessageService _filesMessageService;
    private readonly Global _global;

    public EntryManager(
        IDaoFactory daoFactory,
        FileSecurity fileSecurity,
        GlobalFolderHelper globalFolderHelper,
        PathProvider pathProvider,
        AuthContext authContext,
        FileMarker fileMarker,
        FileUtility fileUtility,
        GlobalStore globalStore,
        CoreBaseSettings coreBaseSettings,
        FilesSettingsHelper filesSettingsHelper,
        UserManager userManager,
        ILogger<EntryManager> logger,
        FileShareLink fileShareLink,
        DocumentServiceHelper documentServiceHelper,
        ThirdpartyConfiguration thirdpartyConfiguration,
        DocumentServiceConnector documentServiceConnector,
        LockerManager lockerManager,
        BreadCrumbsManager breadCrumbsManager,
        SettingsManager settingsManager,
        IServiceProvider serviceProvider,
        ICache cache,
        FileTrackerHelper fileTracker,
        EntryStatusManager entryStatusManager,
        ThirdPartySelector thirdPartySelector,
        IHttpClientFactory clientFactory,
        FilesMessageService filesMessageService,
        ThumbnailSettings thumbnailSettings,
        Global global)
    {
        _daoFactory = daoFactory;
        _fileSecurity = fileSecurity;
        _globalFolderHelper = globalFolderHelper;
        _pathProvider = pathProvider;
        _authContext = authContext;
        _fileMarker = fileMarker;
        _fileUtility = fileUtility;
        _globalStore = globalStore;
        _coreBaseSettings = coreBaseSettings;
        _filesSettingsHelper = filesSettingsHelper;
        _userManager = userManager;
        _fileShareLink = fileShareLink;
        _documentServiceHelper = documentServiceHelper;
        _thirdpartyConfiguration = thirdpartyConfiguration;
        _documentServiceConnector = documentServiceConnector;
        _lockerManager = lockerManager;
        _breadCrumbsManager = breadCrumbsManager;
        _settingsManager = settingsManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cache = cache;
        _fileTracker = fileTracker;
        _entryStatusManager = entryStatusManager;
        _clientFactory = clientFactory;
        _global = global;
        _filesMessageService = filesMessageService;
        _thirdPartySelector = thirdPartySelector;
        _thumbnailSettings = thumbnailSettings;
    }

    public async Task<IEnumerable<FileEntry>> GetEntriesAsync<T>(Folder<T> parent, int from, int count, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent, bool withSubfolders, OrderBy orderBy, SearchArea searchArea = SearchArea.Active, IEnumerable<string> tagNames = null)
    {
        return await GetEntriesAsync(parent, from, count, new[] { filter }, subjectGroup, subjectId, searchText, searchInContent, withSubfolders, orderBy, searchArea, tagNames);
    }

    public async Task<IEnumerable<FileEntry>> GetEntriesAsync<T>(Folder<T> parent, int from, int count, IEnumerable<FilterType> filterTypes, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent, bool withSubfolders, OrderBy orderBy, SearchArea searchArea = SearchArea.Active, IEnumerable<string> tagNames = null)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent), FilesCommonResource.ErrorMassage_FolderNotFound);
        }

        if (parent.ProviderEntry && !_filesSettingsHelper.EnableThirdParty)
        {
            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFolder);
        }

        if (parent.RootFolderType == FolderType.Privacy && (!PrivacyRoomSettings.IsAvailable() || !PrivacyRoomSettings.GetEnabled(_settingsManager)))
        {
            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_ReadFolder);
        }

        var fileSecurity = _fileSecurity;
        var entries = Enumerable.Empty<FileEntry>();

        searchInContent = searchInContent && !filterTypes.Contains(FilterType.ByExtension) && !Equals(parent.Id, _globalFolderHelper.FolderTrash);

        if (parent.FolderType == FolderType.Projects && parent.Id.Equals(await _globalFolderHelper.FolderProjectsAsync))
        {
            //TODO
            //var apiServer = new ASC.Api.ApiServer();
            //var apiUrl = string.Format("{0}project/maxlastmodified.json", SetupInfo.WebApiBaseUrl);

            //const string responseBody = null;// apiServer.GetApiResponse(apiUrl, "GET");
            //if (responseBody != null)
            //{
            //    JObject responseApi;

            //    Dictionary<int, KeyValuePair<int, string>> folderIDProjectTitle = null;

            //    if (folderIDProjectTitle == null)
            //    {
            //        //apiUrl = string.Format("{0}project/filter.json?sortBy=title&sortOrder=ascending&status=open&fields=id,title,security,projectFolder", SetupInfo.WebApiBaseUrl);

            //        responseApi = JObject.Parse(""); //Encoding.UTF8.GetString(Convert.FromBase64String(apiServer.GetApiResponse(apiUrl, "GET"))));

            //        var responseData = responseApi["response"];

            //        if (!(responseData is JArray)) return (entries, total);

            //        folderIDProjectTitle = new Dictionary<int, KeyValuePair<int, string>>();
            //        foreach (JObject projectInfo in responseData.Children().OfType<JObject>())
            //        {
            //            var projectID = projectInfo["id"].Value<int>();
            //            var projectTitle = Global.ReplaceInvalidCharsAndTruncate(projectInfo["title"].Value<string>());

            //            if (projectInfo.TryGetValue("security", out var projectSecurityJToken))
            //            {
            //                var projectSecurity = projectInfo["security"].Value<JObject>();
            //                if (projectSecurity.TryGetValue("canReadFiles", out var projectCanFileReadJToken))
            //                {
            //                    if (!projectSecurity["canReadFiles"].Value<bool>())
            //                    {
            //                        continue;
            //                    }
            //                }
            //            }

            //            int projectFolderID;
            //            if (projectInfo.TryGetValue("projectFolder", out var projectFolderIDjToken))
            //                projectFolderID = projectFolderIDjToken.Value<int>();
            //            else
            //                projectFolderID = await FilesIntegration.RegisterBunchAsync<int>("projects", "project", projectID.ToString());

            //            if (!folderIDProjectTitle.ContainsKey(projectFolderID))
            //                folderIDProjectTitle.Add(projectFolderID, new KeyValuePair<int, string>(projectID, projectTitle));

            //            Cache.Remove("documents/folders/" + projectFolderID);
            //            Cache.Insert("documents/folders/" + projectFolderID, projectTitle, TimeSpan.FromMinutes(30));
            //        }
            //    }

            //    var rootKeys = folderIDProjectTitle.Keys.ToArray();
            //    if (filter == FilterType.None || filter == FilterType.FoldersOnly)
            //    {
            //        var folders = await DaoFactory.GetFolderDao<int>().GetFoldersAsync(rootKeys, filter, subjectGroup, subjectId, searchText, withSubfolders, false).ToListAsync();

            //        var emptyFilter = string.IsNullOrEmpty(searchText) && filter == FilterType.None && subjectId == Guid.Empty;
            //        if (!emptyFilter)
            //        {
            //            var projectFolderIds =
            //                folderIDProjectTitle
            //                    .Where(projectFolder => string.IsNullOrEmpty(searchText)
            //                                            || (projectFolder.Value.Value ?? "").ToLower().Trim().Contains(searchText.ToLower().Trim()))
            //                    .Select(projectFolder => projectFolder.Key);

            //            folders.RemoveAll(folder => rootKeys.Contains(folder.ID));

            //            var projectFolders = await DaoFactory.GetFolderDao<int>().GetFoldersAsync(projectFolderIds.ToList(), filter, subjectGroup, subjectId, null, false, false).ToListAsync();
            //            folders.AddRange(projectFolders);
            //        }

            //        folders.ForEach(x =>
            //            {
            //                x.Title = folderIDProjectTitle.ContainsKey(x.ID) ? folderIDProjectTitle[x.ID].Value : x.Title;
            //                x.FolderUrl = folderIDProjectTitle.ContainsKey(x.ID) ? PathProvider.GetFolderUrlAsync(x, folderIDProjectTitle[x.ID].Key).Result : string.Empty;
            //            });

            //        if (withSubfolders)
            //        {
            //            entries = entries.Concat(await fileSecurity.FilterReadAsync(folders));
            //        }
            //        else
            //        {
            //            entries = entries.Concat(folders);
            //        }
            //    }

            //    if (filter != FilterType.FoldersOnly && withSubfolders)
            //    {
            //        var files = await DaoFactory.GetFileDao<int>().GetFilesAsync(rootKeys, filter, subjectGroup, subjectId, searchText, searchInContent);
            //        entries = entries.Concat(await fileSecurity.FilterReadAsync(files))
            //    }
            //}

            //CalculateTotal();
        }
        else if (parent.FolderType == FolderType.SHARE)
        {
            //share
            var shared = await fileSecurity.GetSharesForMeAsync(filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, searchInContent, withSubfolders);

            entries = entries.Concat(shared);

            CalculateTotal();
        }
        else if (parent.FolderType == FolderType.Recent)
        {
            var files = await GetRecentAsyncEnumerable(filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, searchInContent);
            entries = entries.Concat(files);

            CalculateTotal();
        }
        else if (parent.FolderType == FolderType.Favorites)
        {
            var (files, folders) = await GetFavoritesAsync(filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, searchInContent);

            entries = entries.Concat(folders);
            entries = entries.Concat(files);

            CalculateTotal();
        }
        else if (parent.FolderType == FolderType.Templates)
        {
            var folderDao = _daoFactory.GetFolderDao<T>();
            var fileDao = _daoFactory.GetFileDao<T>();
            var files = await GetTemplatesAsyncEnumerable(folderDao, fileDao, filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, searchInContent).ToListAsync();
            entries = entries.Concat(files);

            CalculateTotal();
        }
        else if (parent.FolderType == FolderType.Privacy)
        {
            var folderDao = _daoFactory.GetFolderDao<T>();
            var fileDao = _daoFactory.GetFileDao<T>();

            var folders = folderDao.GetFoldersAsync(parent.Id, orderBy, filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, withSubfolders);
            var files = fileDao.GetFilesAsync(parent.Id, orderBy, filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, searchInContent, withSubfolders);
            //share
            var shared = fileSecurity.GetPrivacyForMeAsync(filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, searchInContent, withSubfolders);

            var task1 = fileSecurity.FilterReadAsync(folders).ToListAsync();
            var task2 = fileSecurity.FilterReadAsync(files).ToListAsync();
            var task3 = shared.ToListAsync();


            entries = entries.Concat(await task1);
            entries = entries.Concat(await task2);
            entries = entries.Concat(await task3);

            CalculateTotal();
        }
        else if (parent.FolderType == FolderType.VirtualRooms && !parent.ProviderEntry)
        {
            entries = await fileSecurity.GetVirtualRoomsAsync(filterTypes, subjectId, searchText, searchInContent, withSubfolders, orderBy, searchArea, tagNames);

            CalculateTotal();
        }
        else
        {
            if (parent.FolderType == FolderType.TRASH)
            {
                withSubfolders = false;
            }

            var folders = _daoFactory.GetFolderDao<T>().GetFoldersAsync(parent.Id, orderBy, filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, withSubfolders);
            var files = _daoFactory.GetFileDao<T>().GetFilesAsync(parent.Id, orderBy, filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, searchInContent, withSubfolders);

            var task1 = fileSecurity.FilterReadAsync(folders).ToListAsync();
            var task2 = fileSecurity.FilterReadAsync(files).ToListAsync();

            if (filterTypes.FirstOrDefault() == FilterType.None || filterTypes.FirstOrDefault() == FilterType.FoldersOnly)
            {
                var folderList = GetThirpartyFoldersAsyncEnumerable(parent, searchText);
                var thirdPartyFolder = FilterEntriesEnumerable(folderList, filterTypes.FirstOrDefault(), subjectGroup, subjectId, searchText, searchInContent);

                var task3 = thirdPartyFolder.ToListAsync();

                entries = entries.Concat(await task1);
                entries = entries.Concat(await task2);
                entries = entries.Concat(await task3);
            }
            else
            {
                entries = entries.Concat(await task1);
                entries = entries.Concat(await task2);
            }
        }

        IEnumerable<FileEntry> data = entries.ToList();

        if (orderBy.SortedBy != SortedByType.New)
        {
            if (parent.FolderType != FolderType.Recent)
            {
                data = SortEntries<T>(data, orderBy);
            }

            if (0 < from)
            {
                data = data.Skip(from);
            }

            if (0 < count)
            {
                data = data.Take(count);
            }
        }

        data = await _fileMarker.SetTagsNewAsync(parent, data);

        //sorting after marking
        if (orderBy.SortedBy == SortedByType.New)
        {
            data = SortEntries<T>(data, orderBy);

            if (0 < from)
            {
                data = data.Skip(from);
            }

            if (0 < count)
            {
                data = data.Take(count);
            }
        }

        await _entryStatusManager.SetFileStatusAsync(data.Where(r => r != null && r.FileEntryType == FileEntryType.File));
        await _entryStatusManager.SetIsFavoriteFoldersAsync(entries.Where(r => r != null && r.FileEntryType == FileEntryType.Folder).ToList());

        return data;

        void CalculateTotal()
        {
            foreach (var f in entries)
            {
                if (f is IFolder fold)
                {
                    parent.FilesCount += fold.FilesCount;
                    parent.FoldersCount += fold.FoldersCount + 1;
                }
                else
                {
                    parent.FilesCount += 1;
                }
            }
        }
    }

    public async Task<IEnumerable<FileEntry<T>>> GetTemplatesAsync<T>(IFolderDao<T> folderDao, IFileDao<T> fileDao, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
    {
        var tagDao = _daoFactory.GetTagDao<T>();
        var tags = tagDao.GetTagsAsync(_authContext.CurrentAccount.ID, TagType.Template);

        var fileIds = await tags.Where(tag => tag.EntryType == FileEntryType.File).Select(tag => (T)Convert.ChangeType(tag.EntryId, typeof(T))).ToArrayAsync();

        var filesAsync = fileDao.GetFilesFilteredAsync(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent);
        IEnumerable<FileEntry<T>> files = await filesAsync.Where(file => file.RootFolderType != FolderType.TRASH).ToListAsync();
        files = await _fileSecurity.FilterReadAsync(files);

        await CheckFolderIdAsync(folderDao, files);

        return files;
    }

    public async IAsyncEnumerable<FileEntry<T>> GetTemplatesAsyncEnumerable<T>(IFolderDao<T> folderDao, IFileDao<T> fileDao, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
    {
        var tagDao = _daoFactory.GetTagDao<T>();
        var tags = tagDao.GetTagsAsync(_authContext.CurrentAccount.ID, TagType.Template);

        var fileIds = await tags.Where(tag => tag.EntryType == FileEntryType.File).Select(tag => (T)Convert.ChangeType(tag.EntryId, typeof(T))).ToArrayAsync();

        var filesAsync = fileDao.GetFilesFilteredAsync(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent);
        IAsyncEnumerable<FileEntry<T>> files = filesAsync.Where(file => file.RootFolderType != FolderType.TRASH);
        files = _fileSecurity.FilterReadAsync(files);

        await CheckFolderIdAsyncEnumerable(folderDao, files);

        await foreach (var file in files)
        {
            yield return file;
        }
    }

    public async Task<IEnumerable<Folder<string>>> GetThirpartyFoldersAsync<T>(Folder<T> parent, string searchText = null)
    {
        var folderList = new List<Folder<string>>();

        if ((parent.Id.Equals(_globalFolderHelper.FolderMy) || parent.Id.Equals(await _globalFolderHelper.FolderCommonAsync))
            && _thirdpartyConfiguration.SupportInclusion(_daoFactory)
            && (_filesSettingsHelper.EnableThirdParty
                || _coreBaseSettings.Personal))
        {
            var providerDao = _daoFactory.ProviderDao;
            if (providerDao == null)
            {
                return folderList;
            }

            var fileSecurity = _fileSecurity;

            var providers = providerDao.GetProvidersInfoAsync(parent.RootFolderType, searchText);

            await foreach (var providerInfo in providers)
            {
                var fake = GetFakeThirdpartyFolder(providerInfo, parent.Id.ToString());
                if (await fileSecurity.CanReadAsync(fake))
                {
                    folderList.Add(fake);
                }
            }

            if (folderList.Count > 0)
            {
                var securityDao = _daoFactory.GetSecurityDao<string>();
                var pureShareRecords = await securityDao.GetPureShareRecordsAsync(folderList);
                var ids = pureShareRecords
                //.Where(x => x.Owner == SecurityContext.CurrentAccount.ID)
                .Select(x => x.EntryId).Distinct();

                foreach (var id in ids)
                {
                    folderList.First(y => y.Id.Equals(id)).Shared = true;
                }
            }
        }

        return folderList;
    }

    public async IAsyncEnumerable<Folder<string>> GetThirpartyFoldersAsyncEnumerable<T>(Folder<T> parent, string searchText = null)
    {
        var folderList = AsyncEnumerable.Empty<Folder<string>>();

        if ((parent.Id.Equals(_globalFolderHelper.FolderMy) || parent.Id.Equals(await _globalFolderHelper.FolderCommonAsync))
            && _thirdpartyConfiguration.SupportInclusion(_daoFactory)
            && (_filesSettingsHelper.EnableThirdParty
                || _coreBaseSettings.Personal))
        {
            var providerDao = _daoFactory.ProviderDao;
            if (providerDao == null)
            {
                yield break;
            }

            var fileSecurity = _fileSecurity;

            var providers = providerDao.GetProvidersInfoAsync(parent.RootFolderType, searchText);

            folderList = providers
                .Select(providerInfo => GetFakeThirdpartyFolder(providerInfo, parent.Id.ToString()))
                .WhereAwait(async fake => await fileSecurity.CanReadAsync(fake));

            //var securityDao = _daoFactory.GetSecurityDao<string>();
            //var pureShareRecords = securityDao.GetPureShareRecordsAsyncEnumerable(folderList);
            //var ids = pureShareRecords
            //   //.Where(x => x.Owner == SecurityContext.CurrentAccount.ID)
            //   .Select(x => x.EntryId).Distinct();

            //folderList.Intersect(ids, y => dfs);


            //if (folderList.Count > 0)
            //{
            //    var securityDao = _daoFactory.GetSecurityDao<string>();
            //    var pureShareRecords = securityDao.GetPureShareRecordsAsyncEnumerable(folderList);
            //    var ids = pureShareRecords
            //    //.Where(x => x.Owner == SecurityContext.CurrentAccount.ID)
            //    .Select(x => x.EntryId).Distinct();

            //    await foreach (var id in ids)
            //    {
            //        folderList.First(y => y.Id.Equals(id)).Shared = true;
            //    }
            //}
        }

        await foreach (var e in folderList)
        {
            yield return e;
        }
    }

    public async Task<IEnumerable<FileEntry>> GetRecentAsync(FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
    {
        var tagDao = _daoFactory.GetTagDao<int>();
        var tags = await tagDao.GetTagsAsync(_authContext.CurrentAccount.ID, TagType.Recent).ToListAsync();

        var fileIds = tags.Where(tag => tag.EntryType == FileEntryType.File).Select(r => r.EntryId).ToList();

        var files = await GetRecentByIdsAsync(fileIds.OfType<int>(), filter, subjectGroup, subjectId, searchText, searchInContent);
        files = files.Concat(await GetRecentByIdsAsync(fileIds.OfType<string>(), filter, subjectGroup, subjectId, searchText, searchInContent));

        var listFileIds = fileIds.Select(tag => tag.ToString()).ToList();

        return files.OrderBy(file =>
    {
        var fileId = "";
        if (file is File<int> fileInt)
        {
            fileId = fileInt.Id.ToString();
        }
        else if (file is File<string> fileString)
        {
            fileId = fileString.Id;
        }

        return listFileIds.IndexOf(fileId.ToString());
    }).ToList();

        async Task<IEnumerable<FileEntry>> GetRecentByIdsAsync<T>(IEnumerable<T> fileIds, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
        {
            var folderDao = _daoFactory.GetFolderDao<T>();
            var fileDao = _daoFactory.GetFileDao<T>();

            IEnumerable<FileEntry<T>> files = await fileDao.GetFilesFilteredAsync(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent, true).Where(file => file.RootFolderType != FolderType.TRASH).ToListAsync();
            files = await _fileSecurity.FilterReadAsync(files);

            await CheckFolderIdAsync(folderDao, files);

            return files;
        }
    }

    public async Task<IEnumerable<FileEntry>> GetRecentAsyncEnumerable(FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
    {
        var tagDao = _daoFactory.GetTagDao<int>();
        var tags = tagDao.GetTagsAsync(_authContext.CurrentAccount.ID, TagType.Recent);

        var fileIds = await tags.Where(tag => tag.EntryType == FileEntryType.File).Select(r => r.EntryId).ToListAsync();

        var fileIdsInt = Enumerable.Empty<int>();
        var fileIdsString = Enumerable.Empty<string>();
        var listFileIds = new List<string>();

        foreach (var fileId in fileIds)
        {
            if (fileId is int @int)
            {
                fileIdsInt = fileIdsInt.Append(@int);
            }
            if (fileId is string @string)
            {
                fileIdsString = fileIdsString.Append(@string);
            }

            listFileIds.Add(fileId.ToString());
        }

        var files = Enumerable.Empty<FileEntry>();

        var firstTask = GetRecentByIdsAsync(fileIdsInt, filter, subjectGroup, subjectId, searchText, searchInContent).ToListAsync();
        var secondTask = GetRecentByIdsAsync(fileIdsString, filter, subjectGroup, subjectId, searchText, searchInContent).ToListAsync();

        files.Concat(await firstTask);
        files.Concat(await secondTask);

        var result = files.OrderBy(file =>
        {
            var fileId = "";
            if (file is File<int> fileInt)
            {
                fileId = fileInt.Id.ToString();
            }
            else if (file is File<string> fileString)
            {
                fileId = fileString.Id;
            }

            return listFileIds.IndexOf(fileId.ToString());
        });

        return result;

        async IAsyncEnumerable<FileEntry> GetRecentByIdsAsync<T>(IEnumerable<T> fileIds, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
        {
            var folderDao = _daoFactory.GetFolderDao<T>();
            var fileDao = _daoFactory.GetFileDao<T>();

            IAsyncEnumerable<FileEntry<T>> files = fileDao.GetFilesFilteredAsync(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent).Where(file => file.RootFolderType != FolderType.TRASH);
            files = _fileSecurity.FilterReadAsync(files);

            await CheckFolderIdAsyncEnumerable(folderDao, files);

            await foreach (var file in files)
            {
                yield return file;
            }
        }
    }

    public async Task<(IEnumerable<FileEntry>, IEnumerable<FileEntry>)> GetFavoritesAsync(FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
    {
        var tagDao = _daoFactory.GetTagDao<int>();
        var tags = tagDao.GetTagsAsync(_authContext.CurrentAccount.ID, TagType.Favorite);

        var fileIds = tags.Where(tag => tag.EntryType == FileEntryType.File);
        var fileIdsInt = Enumerable.Empty<int>();
        var fileIdsString = Enumerable.Empty<string>();

        await foreach (var fileId in fileIds)
        {
            if (fileId.EntryId is int)
            {
                fileIdsInt.Append((int)fileId.EntryId);
            }
            if (fileId.EntryId is string)
            {
                fileIdsString.Append((string)fileId.EntryId);
            }
        }

        var folderIds = tags.Where(tag => tag.EntryType == FileEntryType.Folder);
        var folderIdsInt = Enumerable.Empty<int>();
        var folderIdsString = Enumerable.Empty<string>();

        await foreach (var folderId in folderIds)
        {
            if (folderId.EntryId is int)
            {
                folderIdsInt.Append((int)folderId.EntryId);
            }
            if (folderId.EntryId is string)
            {
                folderIdsString.Append((string)folderId.EntryId);
            }
        }

        var (filesInt, foldersInt) = await GetFavoritesByIdAsync(fileIdsInt, folderIdsInt, filter, subjectGroup, subjectId, searchText, searchInContent);
        var (filesString, foldersString) = await GetFavoritesByIdAsync(fileIdsString, folderIdsString, filter, subjectGroup, subjectId, searchText, searchInContent);

        var filesTask1 = filesInt.ToListAsync();
        var filesTask2 = filesString.ToListAsync();

        var foldersTask1 = foldersInt.ToListAsync();
        var foldersTask2 = foldersString.ToListAsync();

        var files = await filesTask1;
        files.Concat(await filesTask2);

        var folders = await foldersTask1;
        folders.Concat(await foldersTask2);

        return (files, folders);

        async Task<(IAsyncEnumerable<FileEntry>, IAsyncEnumerable<FileEntry>)> GetFavoritesByIdAsync<T>(IEnumerable<T> fileIds, IEnumerable<T> folderIds, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
        {
            var folderDao = _daoFactory.GetFolderDao<T>();
            var fileDao = _daoFactory.GetFileDao<T>();
            var asyncFolders = folderDao.GetFoldersAsync(folderIds, filter, subjectGroup, subjectId, searchText, false, false);
            var files = AsyncEnumerable.Empty<FileEntry<T>>();
            var folders = AsyncEnumerable.Empty<FileEntry<T>>();
            var asyncFiles = fileDao.GetFilesFilteredAsync(fileIds, filter, subjectGroup, subjectId, searchText, searchInContent, true);
            var fileSecurity = _fileSecurity;

            if (filter == FilterType.None || filter == FilterType.FoldersOnly)
            {
                var tmpFolders = asyncFolders.Where(folder => folder.RootFolderType != FolderType.TRASH);

                folders = fileSecurity.FilterReadAsync(tmpFolders);

                await CheckFolderIdAsyncEnumerable(folderDao, folders);
            }

            if (filter != FilterType.FoldersOnly)
            {
                var tmpFiles = asyncFiles.Where(file => file.RootFolderType != FolderType.TRASH);

                files = fileSecurity.FilterReadAsync(tmpFiles);

                await CheckFolderIdAsyncEnumerable(folderDao, folders);
            }

            return (files, folders);
        }
    }

    public IEnumerable<FileEntry<T>> FilterEntries<T>(IEnumerable<FileEntry<T>> entries, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
    {
        if (entries == null || !entries.Any())
        {
            return entries;
        }

        if (subjectId != Guid.Empty)
        {
            entries = entries.Where(f =>
                                    subjectGroup
                                        ? _userManager.GetUsersByGroup(subjectId).Any(s => s.Id == f.CreateBy)
                                        : f.CreateBy == subjectId
                )
                             .ToList();
        }

        Func<FileEntry<T>, bool> where = null;

        switch (filter)
        {
            case FilterType.SpreadsheetsOnly:
            case FilterType.PresentationsOnly:
            case FilterType.ImagesOnly:
            case FilterType.DocumentsOnly:
            case FilterType.ArchiveOnly:
            case FilterType.FilesOnly:
            case FilterType.MediaOnly:
                where = f => f.FileEntryType == FileEntryType.File && (((File<T>)f).FilterType == filter || filter == FilterType.FilesOnly);
                break;
            case FilterType.FoldersOnly:
                where = f => f.FileEntryType == FileEntryType.Folder;
                break;
            case FilterType.ByExtension:
                var filterExt = (searchText ?? string.Empty).ToLower().Trim();
                where = f => !string.IsNullOrEmpty(filterExt) && f.FileEntryType == FileEntryType.File && FileUtility.GetFileExtension(f.Title).Equals(filterExt);
                break;
        }

        if (where != null)
        {
            entries = entries.Where(where).ToList();
        }

        searchText = (searchText ?? string.Empty).ToLower().Trim();

        if ((!searchInContent || filter == FilterType.ByExtension) && !string.IsNullOrEmpty(searchText))
        {
            entries = entries.Where(f => f.Title.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        return entries;
    }

    public IAsyncEnumerable<FileEntry<T>> FilterEntriesEnumerable<T>(IAsyncEnumerable<FileEntry<T>> entries, FilterType filter, bool subjectGroup, Guid subjectId, string searchText, bool searchInContent)
    {
        if (entries == null)
        {
            return entries;
        }

        if (subjectId != Guid.Empty)
        {
            entries = entries.Where(f =>
                                    subjectGroup
                                        ? _userManager.GetUsersByGroup(subjectId).Any(s => s.Id == f.CreateBy)
                                        : f.CreateBy == subjectId
                );
        }

        Func<FileEntry<T>, bool> where = null;

        switch (filter)
        {
            case FilterType.SpreadsheetsOnly:
            case FilterType.PresentationsOnly:
            case FilterType.ImagesOnly:
            case FilterType.DocumentsOnly:
            case FilterType.ArchiveOnly:
            case FilterType.FilesOnly:
            case FilterType.MediaOnly:
                where = f => f.FileEntryType == FileEntryType.File && (((File<T>)f).FilterType == filter || filter == FilterType.FilesOnly);
                break;
            case FilterType.FoldersOnly:
                where = f => f.FileEntryType == FileEntryType.Folder;
                break;
            case FilterType.ByExtension:
                var filterExt = (searchText ?? string.Empty).ToLower().Trim();
                where = f => !string.IsNullOrEmpty(filterExt) && f.FileEntryType == FileEntryType.File && FileUtility.GetFileExtension(f.Title).Equals(filterExt);
                break;
        }

        if (where != null)
        {
            entries = entries.Where(where);
        }

        searchText = (searchText ?? string.Empty).ToLower().Trim();

        if ((!searchInContent || filter == FilterType.ByExtension) && !string.IsNullOrEmpty(searchText))
        {
            entries = entries.Where(f => f.Title.Contains(searchText, StringComparison.InvariantCultureIgnoreCase));
        }

        return entries;
    }

    public IEnumerable<FileEntry> SortEntries<T>(IEnumerable<FileEntry> entries, OrderBy orderBy)
    {
        if (entries == null || !entries.Any())
        {
            return entries;
        }

        if (orderBy == null)
        {
            orderBy = _filesSettingsHelper.DefaultOrder;
        }

        var c = orderBy.IsAsc ? 1 : -1;
        Comparison<FileEntry> sorter = orderBy.SortedBy switch
        {
            SortedByType.Type => (x, y) =>
            {
                var cmp = 0;
                if (x.FileEntryType == FileEntryType.File && y.FileEntryType == FileEntryType.File)
                {
                    cmp = c * FileUtility.GetFileExtension(x.Title).CompareTo(FileUtility.GetFileExtension(y.Title));
                }

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.Author => (x, y) =>
            {
                var cmp = c * string.Compare(x.CreateByString, y.CreateByString);

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.Size => (x, y) =>
            {
                var cmp = 0;
                if (x.FileEntryType == FileEntryType.File && y.FileEntryType == FileEntryType.File)
                {
                    cmp = c * ((File<T>)x).ContentLength.CompareTo(((File<T>)y).ContentLength);
                }

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.AZ => (x, y) => c * x.Title.EnumerableComparer(y.Title),
            SortedByType.DateAndTime => (x, y) =>
            {
                var cmp = c * DateTime.Compare(x.ModifiedOn, y.ModifiedOn);

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.DateAndTimeCreation => (x, y) =>
            {
                var cmp = c * DateTime.Compare(x.CreateOn, y.CreateOn);

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.New => (x, y) =>
            {
                var isNewSortResult = x.IsNew.CompareTo(y.IsNew);

                return c * (isNewSortResult == 0 ? DateTime.Compare(x.ModifiedOn, y.ModifiedOn) : isNewSortResult);
            }
            ,
            _ => (x, y) => c * x.Title.EnumerableComparer(y.Title),
        };
        if (orderBy.SortedBy != SortedByType.New)
        {
            var pinnedRooms = new List<FileEntry>();

            if (!_coreBaseSettings.DisableDocSpace)
            {
                Func<FileEntry, bool> filter = (e) =>
                {
                    if (e.FileEntryType != FileEntryType.Folder)
                    {
                        return false;
                    }

                    if (((IFolder)e).Pinned)
                    {
                        return true;
                    }

                    return false;
                };

                pinnedRooms = entries.Where(filter).ToList();
            }

            // folders on top
            var folders = entries.Where(r => r.FileEntryType == FileEntryType.Folder).Except(pinnedRooms).ToList();
            var files = entries.Where(r => r.FileEntryType == FileEntryType.File).ToList();
            pinnedRooms.Sort(sorter);
            folders.Sort(sorter);
            files.Sort(sorter);

            return pinnedRooms.Concat(folders).Concat(files);
        }

        var result = entries.ToList();

        result.Sort(sorter);

        return result;
    }

    public IAsyncEnumerable<FileEntry> SortEntriesAsync<T>(IAsyncEnumerable<FileEntry> entries, OrderBy orderBy)
    {
        if (entries == null)
        {
            return entries;
        }

        if (orderBy == null)
        {
            orderBy = _filesSettingsHelper.DefaultOrder;
        }

        var c = orderBy.IsAsc ? 1 : -1;
        Comparison<FileEntry> sorter = orderBy.SortedBy switch
        {
            SortedByType.Type => (x, y) =>
            {
                var cmp = 0;
                if (x.FileEntryType == FileEntryType.File && y.FileEntryType == FileEntryType.File)
                {
                    cmp = c * FileUtility.GetFileExtension(x.Title).CompareTo(FileUtility.GetFileExtension(y.Title));
                }

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.Author => (x, y) =>
            {
                var cmp = c * string.Compare(x.CreateByString, y.CreateByString);

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.Size => (x, y) =>
            {
                var cmp = 0;
                if (x.FileEntryType == FileEntryType.File && y.FileEntryType == FileEntryType.File)
                {
                    cmp = c * ((File<T>)x).ContentLength.CompareTo(((File<T>)y).ContentLength);
                }

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.AZ => (x, y) => c * x.Title.EnumerableComparer(y.Title),
            SortedByType.DateAndTime => (x, y) =>
            {
                var cmp = c * DateTime.Compare(x.ModifiedOn, y.ModifiedOn);

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.DateAndTimeCreation => (x, y) =>
            {
                var cmp = c * DateTime.Compare(x.CreateOn, y.CreateOn);

                return cmp == 0 ? x.Title.EnumerableComparer(y.Title) : cmp;
            }
            ,
            SortedByType.New => (x, y) =>
            {
                var isNewSortResult = x.IsNew.CompareTo(y.IsNew);

                return c * (isNewSortResult == 0 ? DateTime.Compare(x.ModifiedOn, y.ModifiedOn) : isNewSortResult);
            }
            ,
            _ => (x, y) => c * x.Title.EnumerableComparer(y.Title),
        };

        var comparer = Comparer<FileEntry>.Create(sorter);

        if (orderBy.SortedBy != SortedByType.New)
        {
            // folders on top
            var folders = entries.Where(r => r.FileEntryType == FileEntryType.Folder);
            var files = entries.Where(r => r.FileEntryType == FileEntryType.File);
            folders.OrderBy(r => r, comparer);
            files.OrderBy(r => r, comparer);

            return folders.Concat(files);
        }


        entries.OrderBy(r => r, comparer);

        return entries;

    }

    public Folder<string> GetFakeThirdpartyFolder(IProviderInfo providerInfo, string parentFolderId = null)
    {
        //Fake folder. Don't send request to third party
        var folder = _serviceProvider.GetService<Folder<string>>();

        folder.ParentId = parentFolderId;

        folder.Id = providerInfo.RootFolderId;
        folder.CreateBy = providerInfo.Owner;
        folder.CreateOn = providerInfo.CreateOn;
        folder.FolderType = FolderType.DEFAULT;
        folder.ModifiedBy = providerInfo.Owner;
        folder.ModifiedOn = providerInfo.CreateOn;
        folder.ProviderId = providerInfo.ID;
        folder.ProviderKey = providerInfo.ProviderKey;
        folder.RootCreateBy = providerInfo.Owner;
        folder.RootId = providerInfo.RootFolderId;
        folder.RootFolderType = providerInfo.RootFolderType;
        folder.Shareable = false;
        folder.Title = providerInfo.CustomerTitle;
        folder.FilesCount = 0;
        folder.FoldersCount = 0;

        return folder;
    }

    public Task<List<FileEntry>> GetBreadCrumbsAsync<T>(T folderId)
    {
        return _breadCrumbsManager.GetBreadCrumbsAsync(folderId);
    }

    public Task<List<FileEntry>> GetBreadCrumbsAsync<T>(T folderId, IFolderDao<T> folderDao)
    {
        return _breadCrumbsManager.GetBreadCrumbsAsync(folderId, folderDao);
    }

    public async Task CheckFolderIdAsync<T>(IFolderDao<T> folderDao, IEnumerable<FileEntry<T>> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.RootFolderType == FolderType.USER
                && entry.RootCreateBy != _authContext.CurrentAccount.ID)
            {
                var folderId = entry.ParentId;
                var folder = await folderDao.GetFolderAsync(folderId);
                if (!await _fileSecurity.CanReadAsync(folder))
                {
                    entry.FolderIdDisplay = await _globalFolderHelper.GetFolderShareAsync<T>();
                }
            }
        }
    }

    public async Task CheckFolderIdAsyncEnumerable<T>(IFolderDao<T> folderDao, IAsyncEnumerable<FileEntry<T>> entries)
    {
        await foreach (var entry in entries)
        {
            if (entry.RootFolderType == FolderType.USER
                && entry.RootCreateBy != _authContext.CurrentAccount.ID)
            {
                var folderId = entry.ParentId;
                var folder = await folderDao.GetFolderAsync(folderId);
                if (!await _fileSecurity.CanReadAsync(folder))
                {
                    entry.FolderIdDisplay = await _globalFolderHelper.GetFolderShareAsync<T>();
                }
            }
        }
    }

    public Task<bool> FileLockedForMeAsync<T>(T fileId, Guid userId = default)
    {
        return _lockerManager.FileLockedForMeAsync(fileId, userId);
    }

    public Task<Guid> FileLockedByAsync<T>(T fileId, ITagDao<T> tagDao)
    {
        return _lockerManager.FileLockedByAsync(fileId, tagDao);
    }

    public async Task<(File<int> file, Folder<int> folderIfNew)> GetFillFormDraftAsync<T>(File<T> sourceFile)
    {
        Folder<int> folderIfNew = null;
        if (sourceFile == null)
        {
            return (null, folderIfNew);
        }

        File<int> linkedFile = null;
        var fileDao = _daoFactory.GetFileDao<int>();
        var sourceFileDao = _daoFactory.GetFileDao<T>();
        var linkDao = _daoFactory.GetLinkDao();

        var fileSecurity = _fileSecurity;

        var linkedId = await linkDao.GetLinkedAsync(sourceFile.Id.ToString());
        if (linkedId != null)
        {
            linkedFile = await fileDao.GetFileAsync(int.Parse(linkedId));
            if (linkedFile == null
                || !await fileSecurity.CanFillFormsAsync(linkedFile)
                || await FileLockedForMeAsync(linkedFile.Id)
                || linkedFile.RootFolderType == FolderType.TRASH)
            {
                await linkDao.DeleteLinkAsync(sourceFile.Id.ToString());
                linkedFile = null;
            }
        }

        if (linkedFile == null)
        {
            var folderId = _globalFolderHelper.FolderMy;
            var folderDao = _daoFactory.GetFolderDao<int>();
            folderIfNew = await folderDao.GetFolderAsync(folderId);
            if (folderIfNew == null)
            {
                throw new Exception(FilesCommonResource.ErrorMassage_FolderNotFound);
            }

            if (!await fileSecurity.CanCreateAsync(folderIfNew))
            {
                throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);
            }

            linkedFile = _serviceProvider.GetService<File<int>>();
            linkedFile.Title = sourceFile.Title;
            linkedFile.ParentId = folderIfNew.Id;
            linkedFile.FileStatus = sourceFile.FileStatus;
            linkedFile.ConvertedType = sourceFile.ConvertedType;
            linkedFile.Comment = FilesCommonResource.CommentCreateFillFormDraft;
            linkedFile.Encrypted = sourceFile.Encrypted;

            using (var stream = await sourceFileDao.GetFileStreamAsync(sourceFile))
            {
                linkedFile.ContentLength = stream.CanSeek ? stream.Length : sourceFile.ContentLength;
                linkedFile = await fileDao.SaveFileAsync(linkedFile, stream);
            }

            await _fileMarker.MarkAsNewAsync(linkedFile);

            await linkDao.AddLinkAsync(sourceFile.Id.ToString(), linkedFile.Id.ToString());
        }

        return (linkedFile, folderIfNew);
    }

    public async Task<bool> CheckFillFormDraftAsync<T>(File<T> linkedFile)
    {
        if (linkedFile == null)
        {
            return false;
        }

        var linkDao = _daoFactory.GetLinkDao();
        var sourceId = await linkDao.GetSourceAsync(linkedFile.Id.ToString());
        if (sourceId == null)
        {
            return false;
        }

        if (int.TryParse(sourceId, out var sId))
        {
            return await CheckAsync(sId);
        }

        return await CheckAsync(sourceId);

        async Task<bool> CheckAsync<T1>(T1 sourceId)
        {
            var fileDao = _daoFactory.GetFileDao<T1>();
            var sourceFile = await fileDao.GetFileAsync(sourceId);
            if (sourceFile == null
                || !await _fileSecurity.CanFillFormsAsync(sourceFile)
                || sourceFile.Access != FileShare.FillForms)
            {
                await linkDao.DeleteLinkAsync(sourceId.ToString());

                return false;
            }

            return true;
        }
    }

    public async Task<bool> SubmitFillForm<T>(File<T> draft)
    {
        if (draft == null)
        {
            return false;
        }

        try
        {
            var linkDao = _daoFactory.GetLinkDao();
            var sourceId = await linkDao.GetSourceAsync(draft.Id.ToString());
            if (sourceId == null)
            {
                throw new Exception("Link source is not found");
            }

            if (int.TryParse(sourceId, out var sId))
            {
                return await SubmitFillFormFromSource(draft, sId);
            }

            return await SubmitFillFormFromSource(draft, sourceId);
        }
        catch (Exception e)
        {
            _logger.WarningWithException(string.Format("Error on submit form {0}", draft.Id), e);
            return false;
        }
    }

    private async Task<bool> SubmitFillFormFromSource<TDraft, TSource>(File<TDraft> draft, TSource sourceId)
    {
        try
        {
            var linkDao = _daoFactory.GetLinkDao();
            var fileSourceDao = _daoFactory.GetFileDao<TSource>();
            var fileDraftDao = _daoFactory.GetFileDao<TDraft>();
            var folderSourceDao = _daoFactory.GetFolderDao<TSource>();
            var folderDraftDao = _daoFactory.GetFolderDao<TDraft>();

            if (sourceId == null)
            {
                throw new Exception("Link source is not found");
            }

            var sourceFile = await fileSourceDao.GetFileAsync(sourceId);
            if (sourceFile == null)
            {
                throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound, draft.Id.ToString());
            }

            if (!_fileUtility.CanWebRestrictedEditing(sourceFile.Title))
            {
                throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);
            }

            var properties = await fileSourceDao.GetProperties(sourceFile.Id);
            if (properties == null
                || properties.FormFilling == null
                || !properties.FormFilling.CollectFillForm)
            {
                throw new Exception(FilesCommonResource.ErrorMassage_BadRequest);
            }

            var folderId = (TSource)Convert.ChangeType(properties.FormFilling.ToFolderId, typeof(TSource));
            if (!Equals(folderId, default(TSource)))
            {
                var folder = await folderSourceDao.GetFolderAsync(folderId);
                if (folder == null)
                {
                    folderId = sourceFile.ParentId;
                }
            }
            else
            {
                folderId = sourceFile.ParentId;
            }

            //todo: think about right to create in folder

            if (!string.IsNullOrEmpty(properties.FormFilling.CreateFolderTitle))
            {
                var newFolderTitle = Global.ReplaceInvalidCharsAndTruncate(properties.FormFilling.CreateFolderTitle);

                var folder = await folderSourceDao.GetFolderAsync(newFolderTitle, folderId);
                if (folder == null)
                {
                    folder = new Folder<TSource> { Title = newFolderTitle, ParentId = folderId };
                    folderId = await folderSourceDao.SaveFolderAsync(folder);

                    folder = await folderSourceDao.GetFolderAsync(folderId);
                    _filesMessageService.Send(folder, MessageInitiator.DocsService, MessageAction.FolderCreated, folder.Title);
                }

                folderId = folder.Id;
            }
            //todo: think about right to create in folder

            var title = properties.FormFilling.GetTitleByMask(sourceFile.Title);

            var submitFile = new File<TSource>
            {
                Title = title,
                ParentId = folderId,
                FileStatus = draft.FileStatus,
                ConvertedType = draft.ConvertedType,
                Comment = FilesCommonResource.CommentSubmitFillForm,
                Encrypted = draft.Encrypted,
            };

            using (var stream = await fileDraftDao.GetFileStreamAsync(draft))
            {
                submitFile.ContentLength = stream.CanSeek ? stream.Length : draft.ContentLength;
                submitFile = await fileSourceDao.SaveFileAsync(submitFile, stream);
            }

            _filesMessageService.Send(submitFile, MessageInitiator.DocsService, MessageAction.FileCreated, submitFile.Title);

            await _fileMarker.MarkAsNewAsync(submitFile);

            return true;

        }
        catch (Exception e)
        {
            _logger.WarningWithException(string.Format("Error on submit form {0}", draft.Id), e);
            return false;
        }
    }

    public async Task<File<T>> SaveEditingAsync<T>(T fileId, string fileExtension, string downloadUri, Stream stream, string doc, string comment = null, bool checkRight = true, bool encrypted = false, ForcesaveType? forcesave = null, bool keepLink = false)
    {
        var newExtension = string.IsNullOrEmpty(fileExtension)
                          ? FileUtility.GetFileExtension(downloadUri)
                          : fileExtension;

        if (!string.IsNullOrEmpty(newExtension))
        {
            newExtension = "." + newExtension.Trim('.');
        }

        var app = _thirdPartySelector.GetAppByFileId(fileId.ToString());
        if (app != null)
        {
            await app.SaveFileAsync(fileId.ToString(), newExtension, downloadUri, stream);

            return null;
        }

        var fileDao = _daoFactory.GetFileDao<T>();
        var check = await _fileShareLink.CheckAsync(doc, false, fileDao);
        var editLink = check.EditLink;
        var file = check.File;
        if (file == null)
        {
            file = await fileDao.GetFileAsync(fileId);
        }

        if (file == null)
        {
            throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
        }

        if (checkRight && !editLink && (!await _fileSecurity.CanEditAsync(file) || _userManager.GetUsers(_authContext.CurrentAccount.ID).IsVisitor(_userManager)))
        {
            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
        }

        if (checkRight && await FileLockedForMeAsync(file.Id))
        {
            throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
        }

        if (checkRight && (!forcesave.HasValue || forcesave.Value == ForcesaveType.None) && _fileTracker.IsEditing(file.Id))
        {
            throw new Exception(FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile);
        }

        if (file.RootFolderType == FolderType.TRASH)
        {
            throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);
        }

        var currentExt = file.ConvertedExtension;
        if (string.IsNullOrEmpty(newExtension))
        {
            newExtension = FileUtility.GetFileExtension(file.Title);
        }

        var replaceVersion = false;
        if (file.Forcesave != ForcesaveType.None)
        {
            if (file.Forcesave == ForcesaveType.User && _filesSettingsHelper.StoreForcesave || encrypted)
            {
                file.Version++;
            }
            else
            {
                replaceVersion = true;
            }
        }
        else
        {
            if (file.Version != 1 || string.IsNullOrEmpty(currentExt))
            {
                file.VersionGroup++;
            }
            else
            {
                var storeTemplate = _globalStore.GetStoreTemplate();

                var path = FileConstant.NewDocPath + Thread.CurrentThread.CurrentCulture + "/";
                if (!await storeTemplate.IsDirectoryAsync(path))
                {
                    path = FileConstant.NewDocPath + "en-US/";
                }

                var fileExt = currentExt != _fileUtility.MasterFormExtension
                    ? _fileUtility.GetInternalExtension(file.Title)
                    : currentExt;

                path += "new" + fileExt;

                //todo: think about the criteria for saving after creation
                if (!await storeTemplate.IsFileAsync(path) || file.ContentLength != await storeTemplate.GetFileSizeAsync("", path))
                {
                    file.VersionGroup++;
                }
            }
            file.Version++;
        }
        file.Forcesave = forcesave ?? ForcesaveType.None;

        if (string.IsNullOrEmpty(comment))
        {
            comment = FilesCommonResource.CommentEdit;
        }

        file.Encrypted = encrypted;

        file.ConvertedType = FileUtility.GetFileExtension(file.Title) != newExtension ? newExtension : null;
        file.ThumbnailStatus = encrypted ? Thumbnail.NotRequired : Thumbnail.Waiting;

        if (file.ProviderEntry && !newExtension.Equals(currentExt))
        {
            if (_fileUtility.ExtsConvertible.ContainsKey(newExtension) && _fileUtility.ExtsConvertible[newExtension].Contains(currentExt))
            {
                if (stream != null)
                {
                    downloadUri = await _pathProvider.GetTempUrlAsync(stream, newExtension);
                    downloadUri = _documentServiceConnector.ReplaceCommunityAdress(downloadUri);
                }

                var key = DocumentServiceConnector.GenerateRevisionId(downloadUri);

                var resultTuple = await _documentServiceConnector.GetConvertedUriAsync(downloadUri, newExtension, currentExt, key, null, CultureInfo.CurrentUICulture.Name, null, null, false);
                downloadUri = resultTuple.ConvertedDocumentUri;

                stream = null;
            }
            else
            {
                file.Id = default;
                file.Title = FileUtility.ReplaceFileExtension(file.Title, newExtension);
            }

            file.ConvertedType = null;
        }

        using (var tmpStream = new MemoryStream())
        {
            if (stream != null)
            {
                await stream.CopyToAsync(tmpStream);
            }
            else
            {

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(downloadUri)
                };

                var httpClient = _clientFactory.CreateClient();
                using var response = await httpClient.SendAsync(request);
                using var editedFileStream = new ResponseStream(response);
                await editedFileStream.CopyToAsync(tmpStream);
            }
            tmpStream.Position = 0;

            file.ContentLength = tmpStream.Length;
            file.Comment = string.IsNullOrEmpty(comment) ? null : comment;
            if (replaceVersion)
            {
                file = await fileDao.ReplaceFileVersionAsync(file, tmpStream);
            }
            else
            {
                file = await fileDao.SaveFileAsync(file, tmpStream);
            }
            if (!keepLink
               || file.CreateBy != _authContext.CurrentAccount.ID
               || !file.IsFillFormDraft)
            {
                var linkDao = _daoFactory.GetLinkDao();
                await linkDao.DeleteAllLinkAsync(file.Id.ToString());
            }
        }

        await _fileMarker.MarkAsNewAsync(file);
        await _fileMarker.RemoveMarkAsNewAsync(file);

        return file;
    }

    public async Task TrackEditingAsync<T>(T fileId, Guid tabId, Guid userId, string doc, bool editingAlone = false)
    {
        bool checkRight;
        if (_fileTracker.GetEditingBy(fileId).Contains(userId))
        {
            checkRight = _fileTracker.ProlongEditing(fileId, tabId, userId, editingAlone);
            if (!checkRight)
            {
                return;
            }
        }

        var fileDao = _daoFactory.GetFileDao<T>();
        var check = await _fileShareLink.CheckAsync(doc, false, fileDao);
        var editLink = check.EditLink;
        var file = check.File;
        if (file == null)
        {
            file = await fileDao.GetFileAsync(fileId);
        }

        if (file == null)
        {
            throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
        }
        if (!editLink
            && (!await _fileSecurity.CanEditAsync(file, userId)
                && !await _fileSecurity.CanCustomFilterEditAsync(file, userId)
                && !await _fileSecurity.CanReviewAsync(file, userId)
                && !await _fileSecurity.CanFillFormsAsync(file, userId)
                && !await _fileSecurity.CanCommentAsync(file, userId)
                || _userManager.GetUsers(userId).IsVisitor(_userManager)))
        {
            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
        }
        if (await FileLockedForMeAsync(file.Id, userId))
        {
            throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
        }

        if (file.RootFolderType == FolderType.TRASH)
        {
            throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);
        }

        checkRight = _fileTracker.ProlongEditing(fileId, tabId, userId, editingAlone);
        if (checkRight)
        {
            _fileTracker.ChangeRight(fileId, userId, false);
        }
    }

    public async Task<File<T>> UpdateToVersionFileAsync<T>(T fileId, int version, string doc = null, bool checkRight = true)
    {
        var fileDao = _daoFactory.GetFileDao<T>();
        if (version < 1)
        {
            throw new ArgumentNullException(nameof(version));
        }

        var (editLink, fromFile, _) = await _fileShareLink.CheckAsync(doc, false, fileDao);

        if (fromFile == null)
        {
            fromFile = await fileDao.GetFileAsync(fileId);
        }

        if (fromFile == null)
        {
            throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
        }

        if (fromFile.Version != version)
        {
            fromFile = await fileDao.GetFileAsync(fromFile.Id, Math.Min(fromFile.Version, version));
        }

        if (fromFile == null)
        {
            throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
        }

        if (checkRight && !editLink && (!await _fileSecurity.CanEditAsync(fromFile) || _userManager.GetUsers(_authContext.CurrentAccount.ID).IsVisitor(_userManager)))
        {
            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
        }

        if (await FileLockedForMeAsync(fromFile.Id))
        {
            throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
        }

        if (checkRight && _fileTracker.IsEditing(fromFile.Id))
        {
            throw new Exception(FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile);
        }

        if (fromFile.RootFolderType == FolderType.TRASH)
        {
            throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);
        }

        if (fromFile.ProviderEntry)
        {
            throw new Exception(FilesCommonResource.ErrorMassage_BadRequest);
        }

        if (fromFile.Encrypted)
        {
            throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);
        }

        var exists = _cache.Get<string>(_updateList + fileId.ToString()) != null;
        if (exists)
        {
            throw new Exception(FilesCommonResource.ErrorMassage_UpdateEditingFile);
        }
        else
        {
            _cache.Insert(_updateList + fileId.ToString(), fileId.ToString(), TimeSpan.FromMinutes(2));
        }

        try
        {
            var currFile = await fileDao.GetFileAsync(fileId);
            var newFile = _serviceProvider.GetService<File<T>>();

            newFile.Id = fromFile.Id;
            newFile.Version = currFile.Version + 1;
            newFile.VersionGroup = currFile.VersionGroup;
            newFile.Title = FileUtility.ReplaceFileExtension(currFile.Title, FileUtility.GetFileExtension(fromFile.Title));
            newFile.FileStatus = currFile.FileStatus;
            newFile.ParentId = currFile.ParentId;
            newFile.CreateBy = currFile.CreateBy;
            newFile.CreateOn = currFile.CreateOn;
            newFile.ModifiedBy = fromFile.ModifiedBy;
            newFile.ModifiedOn = fromFile.ModifiedOn;
            newFile.ConvertedType = fromFile.ConvertedType;
            newFile.Comment = string.Format(FilesCommonResource.CommentRevert, fromFile.ModifiedOnString);
            newFile.Encrypted = fromFile.Encrypted;

            using (var stream = await fileDao.GetFileStreamAsync(fromFile))
            {
                newFile.ContentLength = stream.CanSeek ? stream.Length : fromFile.ContentLength;
                newFile = await fileDao.SaveFileAsync(newFile, stream);
            }

            if (fromFile.ThumbnailStatus == Thumbnail.Created)
            {
                foreach (var size in _thumbnailSettings.Sizes)
                {
                    using (var thumb = await fileDao.GetThumbnailAsync(fromFile, size.Width, size.Height))
                    {
                        await fileDao.SaveThumbnailAsync(newFile, thumb, size.Width, size.Height);
                    }
                }

                newFile.ThumbnailStatus = Thumbnail.Created;
            }

            var linkDao = _daoFactory.GetLinkDao();
            await linkDao.DeleteAllLinkAsync(newFile.Id.ToString());

            await _fileMarker.MarkAsNewAsync(newFile); ;

            await _entryStatusManager.SetFileStatusAsync(newFile);

            newFile.Access = fromFile.Access;

            if (newFile.IsTemplate
                && !_fileUtility.ExtsWebTemplate.Contains(FileUtility.GetFileExtension(newFile.Title), StringComparer.CurrentCultureIgnoreCase))
            {
                var tagTemplate = Tag.Template(_authContext.CurrentAccount.ID, newFile);
                var tagDao = _daoFactory.GetTagDao<T>();
                tagDao.RemoveTags(tagTemplate);

                newFile.IsTemplate = false;
            }

            return newFile;
        }
        catch (Exception e)
        {
            _logger.ErrorUpdateFile(fileId.ToString(), version, e);

            throw new Exception(e.Message, e);
        }
        finally
        {
            _cache.Remove(_updateList + fromFile.Id);
        }
    }

    public async Task<File<T>> CompleteVersionFileAsync<T>(T fileId, int version, bool continueVersion, bool checkRight = true)
    {
        var fileDao = _daoFactory.GetFileDao<T>();
        var fileVersion = version > 0
            ? await fileDao.GetFileAsync(fileId, version)
            : await fileDao.GetFileAsync(fileId);
        if (fileVersion == null)
        {
            throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
        }

        if (checkRight && (!await _fileSecurity.CanEditAsync(fileVersion) || _userManager.GetUsers(_authContext.CurrentAccount.ID).IsVisitor(_userManager)))
        {
            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_EditFile);
        }

        if (await FileLockedForMeAsync(fileVersion.Id))
        {
            throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
        }

        if (fileVersion.RootFolderType == FolderType.TRASH)
        {
            throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);
        }

        if (fileVersion.ProviderEntry)
        {
            throw new Exception(FilesCommonResource.ErrorMassage_BadRequest);
        }

        var lastVersionFile = await fileDao.GetFileAsync(fileVersion.Id);

        if (continueVersion)
        {
            if (lastVersionFile.VersionGroup > 1)
            {
                await fileDao.ContinueVersionAsync(fileVersion.Id, fileVersion.Version);
                lastVersionFile.VersionGroup--;
            }
        }
        else
        {
            if (!_fileTracker.IsEditing(lastVersionFile.Id))
            {
                if (fileVersion.Version == lastVersionFile.Version)
                {
                    lastVersionFile = await UpdateToVersionFileAsync(fileVersion.Id, fileVersion.Version, null, checkRight);
                }

                await fileDao.CompleteVersionAsync(fileVersion.Id, fileVersion.Version);
                lastVersionFile.VersionGroup++;
            }
        }

        await _entryStatusManager.SetFileStatusAsync(lastVersionFile);

        return lastVersionFile;
    }

    public async Task<FileOptions<T>> FileRenameAsync<T>(T fileId, string title)
    {
        var fileDao = _daoFactory.GetFileDao<T>();
        var file = await fileDao.GetFileAsync(fileId);
        if (file == null)
        {
            throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
        }

        if (!await _fileSecurity.CanRenameAsync(file))
        {
            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_RenameFile);
        }

        if (!await _fileSecurity.CanDeleteAsync(file) && _userManager.GetUsers(_authContext.CurrentAccount.ID).IsVisitor(_userManager))
        {
            throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_RenameFile);
        }

        if (await FileLockedForMeAsync(file.Id))
        {
            throw new Exception(FilesCommonResource.ErrorMassage_LockedFile);
        }

        if (file.ProviderEntry && _fileTracker.IsEditing(file.Id))
        {
            throw new Exception(FilesCommonResource.ErrorMassage_UpdateEditingFile);
        }

        if (file.RootFolderType == FolderType.TRASH)
        {
            throw new Exception(FilesCommonResource.ErrorMassage_ViewTrashItem);
        }

        title = Global.ReplaceInvalidCharsAndTruncate(title);

        var ext = FileUtility.GetFileExtension(file.Title);
        if (!string.Equals(ext, FileUtility.GetFileExtension(title), StringComparison.InvariantCultureIgnoreCase))
        {
            title += ext;
        }

        var fileAccess = file.Access;

        var renamed = false;
        if (!string.Equals(file.Title, title))
        {
            var newFileID = await fileDao.FileRenameAsync(file, title);

            file = await fileDao.GetFileAsync(newFileID);
            file.Access = fileAccess;

            await _documentServiceHelper.RenameFileAsync(file, fileDao);

            renamed = true;
        }

        await _entryStatusManager.SetFileStatusAsync(file);

        return new FileOptions<T>
        {
            File = file,
            Renamed = renamed
        };
    }

    public void MarkAsRecent<T>(File<T> file)
    {
        if (file.Encrypted || file.ProviderEntry)
        {
            throw new NotSupportedException();
        }

        var tagDao = _daoFactory.GetTagDao<T>();
        var userID = _authContext.CurrentAccount.ID;

        var tag = Tag.Recent(userID, file);
        tagDao.SaveTags(tag);
    }


    //Long operation
    public async Task DeleteSubitemsAsync<T>(T parentId, IFolderDao<T> folderDao, IFileDao<T> fileDao, ILinkDao linkDao)
    {
        var folders = folderDao.GetFoldersAsync(parentId);
        await foreach (var folder in folders)
        {
            await DeleteSubitemsAsync(folder.Id, folderDao, fileDao, linkDao);

            _logger.InformationDeleteFolder(folder.Id.ToString(), parentId.ToString());
            await folderDao.DeleteFolderAsync(folder.Id);
        }

        var files = fileDao.GetFilesAsync(parentId, null, FilterType.None, false, Guid.Empty, string.Empty, true);
        await foreach (var file in files)
        {
            _logger.InformationDeletefile(file.Id.ToString(), parentId.ToString());
            await fileDao.DeleteFileAsync(file.Id);

            await linkDao.DeleteAllLinkAsync(file.Id.ToString());
        }
    }

    public async Task MoveSharedItemsAsync<T>(T parentId, T toId, IFolderDao<T> folderDao, IFileDao<T> fileDao)
    {
        var fileSecurity = _fileSecurity;

        var folders = folderDao.GetFoldersAsync(parentId);
        await foreach (var folder in folders)
        {
            var shares = await fileSecurity.GetSharesAsync(folder);
            var shared = folder.Shared
                         && shares.Any(record => record.Share != FileShare.Restrict);
            if (shared)
            {
                _logger.InformationMoveSharedFolder(folder.Id.ToString(), parentId.ToString(), toId.ToString());
                await folderDao.MoveFolderAsync(folder.Id, toId, null);
            }
            else
            {
                await MoveSharedItemsAsync(folder.Id, toId, folderDao, fileDao);
            }
        }

        var files = fileDao.GetFilesAsync(parentId, null, FilterType.None, false, Guid.Empty, string.Empty, true)
            .WhereAwait(async file => file.Shared &&
            (await fileSecurity.GetSharesAsync(file)).Any(record => record.Subject != FileConstant.ShareLinkId && record.Share != FileShare.Restrict));

        await foreach (var file in files)
        {
            _logger.InformationMoveSharedFile(file.Id.ToString(), parentId.ToString(), toId.ToString());
            await fileDao.MoveFileAsync(file.Id, toId);
        }
    }

    public static async Task ReassignItemsAsync<T>(T parentId, Guid fromUserId, Guid toUserId, IFolderDao<T> folderDao, IFileDao<T> fileDao)
    {
        var files = await fileDao.GetFilesAsync(parentId, new OrderBy(SortedByType.AZ, true), FilterType.ByUser, false, fromUserId, null, true, true).ToListAsync();
        var fileIds = files.Where(file => file.CreateBy == fromUserId).Select(file => file.Id);

        await fileDao.ReassignFilesAsync(fileIds.ToArray(), toUserId);

        var folderIds = await folderDao.GetFoldersAsync(parentId, new OrderBy(SortedByType.AZ, true), FilterType.ByUser, false, fromUserId, null, true)
                                 .Where(folder => folder.CreateBy == fromUserId).Select(folder => folder.Id)
                                 .ToListAsync();

        await folderDao.ReassignFoldersAsync(folderIds.ToArray(), toUserId);
    }
}
