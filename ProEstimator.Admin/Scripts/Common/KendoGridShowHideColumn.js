// Grid column selection

function ShowHideLinkClick() {

        $('#' + checkboxListID).html('');

        // loop the outer array
        for (var i = 0; i < _columnSelections.length; i++) {

            var name = _columnSelections[i]["Name"];
            var text = _columnSelections[i]["HeaderText"];
            var checked = _columnSelections[i]["Visible"];

            if(name != '' &&  name != undefined)
            {
                addCheckbox(name, text, checked);
            }
        }

        $('#' + checkboxListID).append('<input onclick="SaveColumnDialog();" style="background-color: #1d69a6;color: #FFF; margin:' + saveMargin + '; border-radius:5px"  id="SaveColumnDialog" name="SaveColumnDialog" type="button" value="Save" />');
        $('#' + checkboxListID).append('<input onclick="CancelColumnDialog();" style="background-color: #1d69a6;color: #FFF; margin:' + cancelMargin + '; border-radius:5px"  id="CancelColumnDialog" name="CancelColumnDialog" type="button" value="Cancel"/>');

        $('.menu').toggle("slide");
}

function CancelColumnDialog() {
    $('.menu').toggle("slide");
}

function SaveColumnDialog() {
    SaveGridShowHideColumn(false);
    console.log(_columnSelections);

    var urlToSave = "/" + controllerName + "/" + saveActionMethodName;

    ShowLoadingOverlay();

    $.ajax({
        url: urlToSave,
        type: "POST",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ "loginID": loginID, "gridControlID": kendoGridID, "gridMappingVMList": _columnSelections }),
        success: function (data)
        {
            HideLoadingOverlay();

            SetGridShowHideColumn();
            $('.menu').toggle("slide"); // toogle menu popup
        }
    });
}

function SetGridShowHideColumn() {
    var grid = $("#" + kendoGridID).data("kendoGrid");

    if ($("#" + showHideLinkID).is(":visible"))
    {
        for (var i = 0; i < _columnSelections.length; i++)
        {
            if (_columnSelections[i].Visible == true)
            {
                grid.showColumn(_columnSelections[i].Name);
            }
            else
            {
                grid.hideColumn(_columnSelections[i].Name);
            }                
        }
    }
    else
    {
        for (var i = 0; i < _columnSelections.length; i++)
        {
            grid.hideColumn(_columnSelections[i].Name);
        }

        grid.showColumn("Name");
    }

    for (var i = 0; i < _columnSelections.length - 1; i++) 
    {
        for (var j = 0; j < grid.columns.length; j++) {
            if (grid.columns[j].field == _columnSelections[i].Name) {
                grid.reorderColumn(_columnSelections[i].SortOrderIndex, grid.columns[j]);
                break;
            }
        }
    }
}

function LoadGridShowHideColumnInfo() {

    var url = "/" + controllerName + "/" + getActionMethodName;

    $.getJSON(url, { loginID: loginID, gridControlID: kendoGridID }, function (data) {

        //HideLoadingOverlay();

        if (data.Success) {
            _columnSelections = data.ResultObject;
            SetGridShowHideColumn();
        }
        else {
            alert(data.ErrorMessage);
        }
    });
}

function addCheckbox(name, text, checked) {
    var container = $('#' + checkboxListID);
    var inputs = container.find('input');
    var id = 1;
    if(inputs != undefined)
        id = inputs.length+1;

    if(checked == true)
        $('#' + checkboxListID).append('<input type="checkbox" checked style="margin: 5px;" id=cb' + id + ' value = ' + name + ' /> ' + text + '<br />');
    else
        $('#' + checkboxListID).append('<input type="checkbox" style="margin: 5px;" id=cb' + id + ' value = ' + name + ' /> ' + text + '<br />');
}

function SaveGridShowHideColumn(toggleMenu) {
    $('#' + checkboxListID + ' :checked').each(function () {
        for (var i = 0; i < _columnSelections.length; i++) {

            name = _columnSelections[i]["Name"];
            if($(this).val() == name)
            {
                _columnSelections[i]["Visible"] = true;
            }
        }
    });

    $('#' + checkboxListID + ' :not(:checked)').each(function () {
        for (var i = 0; i < _columnSelections.length; i++) {

            name = _columnSelections[i]["Name"];
            if($(this).val() == name)
            {
                _columnSelections[i]["Visible"] = false;
            }
        }
    });

    if(toggleMenu)
    {
        for (var i = 0; i < _columnSelections.length; i++) {

            var grid = $("#" + kendoGridID).data("kendoGrid");

            if(_columnSelections[i]["Visible"]==true)
                grid.showColumn(_columnSelections[i]["Name"]);
            else
                grid.hideColumn(_columnSelections[i]["Name"]);
        }

        $('.menu').toggle("slide");
    }
}