function SubmitAndRedirect(path) {
    if (typeof BeforeRedirect == "function") {
        BeforeRedirect();
    }

    var hiddenData = $("#redirectData");
    var button = $("#submitButton");
    
    if (((_isImpersonated && _autoSaveTurnedOnTechSupport) || (_adminIsImpersonating && _autoSaveTurnedOnSiteUser)
                            || (!_isImpersonated && !_adminIsImpersonating && _autoSaveTurnedOnSiteUser)) && hiddenData.length && button.length) {
    //if ((_autoSaveTurnedOnTechSupport || _autoSaveTurnedOnSiteUser) && hiddenData.length && button.length) {
        $("#redirectData").val(path);
        $("#submitButton").trigger("click");
    }
    else {
        location.href = path;
    }
}   

function SubmitAndRedirectAction(action) {
    DoRedirect("", action);
}

function DoRedirect(controller, action) {
    if (typeof BeforeRedirect == "function")
    {
        BeforeRedirect();
    }

    var hiddenData = $("#redirectData");
    var button = $("#submitButton");

    if (hiddenData.length && button.length) {
        var data = "";
        if (controller != "") {
            data = controller + ":" + action;
        }
        else{
            data = action;
        }

        $("#redirectData").val(data);
        $("#submitButton").trigger("click");
    }
    else {
        var link = "";
        if (controller != "") {
            link = "/" + controller;
        }

        if (link.length > 0) {
            link += "/";
        }

        link += action;

        location.href = link;
    }
}