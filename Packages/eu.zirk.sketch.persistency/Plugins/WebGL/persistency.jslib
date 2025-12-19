var HandleIO = {
    SyncFiles : function()
    {
        FS.syncfs(false,function (err) {
            if (err) alert(err);
        });
    }
};
mergeInto(LibraryManager.library, HandleIO);