var _currentVendorID = 0;
var _currentVendorType = 0;
var _public = true;

var _previouskendoEstimateIDClicked = '';

function ShowMessage(message)
{
    var newP = $(document.createElement("p"));
    newP.text(message);

    $(".message-container").append(newP);

    window.setTimeout(function () {
        $(".message-container").empty();
    }, 4000);
}

function loadForEditing(vendorid) {
    $.getJSON('/Vendors/GetVendor', { userID: _userID, vendorID: vendorid }, function (data) {
        LoadVendorIntoForm(data.Vendor);
    });
}

function LoadVendorIntoForm(vendor)
{
    _currentVendorID = vendor.ID;

    $("#txtCompanyName").val(vendor.CompanyName);
    $("#txtFax").val(vendor.FaxNumber);
    $("#txtFirstName").val(vendor.FirstName);
    $("#txtLastName").val(vendor.LastName);
    $("#txtEmail").val(vendor.Email);
    $("#txtMobileNumber").val(vendor.MobilePhone);
    $("#txtWorkNumber").val(vendor.WorkPhone);
    $("#txtAddress1").val(vendor.Address1);
    $("#txtAddress2").val(vendor.Address2);
    $("#txtCity").val(vendor.City);
    $("#txtState").val(vendor.State);
    $("#txtZip").val(vendor.Zip);
    $("#txtExtension").val(vendor.Extension);
    $("#ddlTimeZone").val(vendor.TimeZone)
    $("#chkUniversal").prop('checked', vendor.Universal);
    $("#txtFederalTaxID").val(vendor.FederalTaxID);
    $("#txtLicenseNumber").val(vendor.LicenseNumber);
    $("#txtBarNumber").val(vendor.BarNumber);
    $("#txtRegistrationNumber").val(vendor.RegistrationNumber);

    if (vendor.LoginsID == _loginID)
    {
        $(".save-buttons").show();
        $(".public-message").hide();

        EnableForm();

        $(".btnSave").val(_pageStrings.SaveVendorInfo);
    }
    else
    {
        $(".save-buttons").hide();
        $(".public-message").show();

        DisableForm();
    }

    UpdateFormattedText();
    ValidateEmail("txtEmail");

    ShowVendorForm();
}

function ClearVendorForm() {
    _currentVendorID = 0;
    $("#txtCompanyName").val("");
    $("#txtFax").val("");
    $("#txtFirstName").val("");
    $("#txtLastName").val("");
    $("#txtEmail").val("");
    $("#txtMobileNumber").val("");
    $("#txtWorkNumber").val("");
    $("#txtAddress1").val("");
    $("#txtAddress2").val("");
    $("#txtCity").val("");
    $("#txtState").val("");
    $("#txtZip").val("");
    $("#ddlTimeZone").val("Eastern")
    $("#chkUniversal").prop("checked", false);
    $("#txtExtension").val("");

    $("#txtFederalTaxID").val("");
    $("#txtLicenseNumber").val("");
    $("#txtBarNumber").val("");
    $("#txtRegistrationNumber").val("");

    $(".save-buttons").show();
    $(".public-message").hide();

    $(".btnDelete").hide();
    $(".btnSave").val(_pageStrings.AddVendorInfo);

    EnableForm();
    ShowVendorForm();

    $("#vendor-grid").find(".k-state-selected").removeClass("k-state-selected");
}

function VendorToVM() {
    var vendor = {
        ID: _currentVendorID,
        CompanyName: $("#txtCompanyName").val(),
        FaxNumber: $("#txtFax").val(),
        FirstName: $("#txtFirstName").val(),
        LastName: $("#txtLastName").val(),
        Email: $("#txtEmail").val(),
        MobilePhone: $("#txtMobileNumber").val(),
        WorkPhone: $("#txtWorkNumber").val(),
        Address1: $("#txtAddress1").val(),
        Address2: $("#txtAddress2").val(),
        City: $("#txtCity").val(),
        State: $("#txtState").val(),
        Zip: $("#txtZip").val(),
        Extension: $("#txtExtension").val(),
        TimeZone: $("#ddlTimeZone").val(),
        Universal: $("#chkUniversal").prop('checked'),
        FederalTaxID: $("#txtFederalTaxID").val(),
        LicenseNumber: $("#txtLicenseNumber").val(),
        BarNumber: $("#txtBarNumber").val(),
        RegistrationNumber: $("#txtRegistrationNumber").val(),
        LoginsID: _loginID,
        Type: _currentVendorType
    };

    return vendor;
}

function DisableForm()
{
    $(".form-control").prop("disabled", true);
    $(".form-control").css("opacity", 0.7);
}

function EnableForm()
{
    $(".form-control").prop("disabled", false);
    $(".form-control").css("opacity", 1);
}

function ShowVendorForm() {
    $(".vendor-container").fadeIn();
}

function HideVendorForm() {
    $(".vendor-container").fadeOut();
}

function CheckIfRepairFacilitySelected() {

    $("#divFederalTaxID").hide();
    $("#divLicenseNumber").hide();
    $("#divBarNumber").hide();
    $("#divRegistrationNumber").hide();

    if (_currentVendorType == 8) {
        $("#divFederalTaxID").show();
        $("#divLicenseNumber").show();
        $("#divBarNumber").show();
        $("#divRegistrationNumber").show();
    }
}

function TrimSpaces(x) {
    return x.replace(/^\s+|\s+$/gm,'');
}

function ShowPublicVendors() {
    _public = true;

    $("#vendorsTabBar").find(".active").removeClass("active");
    $("#tabButtonPublicVendors").addClass("active");
    $(".myVendorsStuff").hide();
    $(".extra-note").show();

    var grid = $("#vendor-grid").data("kendoGrid");
    if (grid) {
        grid.showColumn(1);
    }

    RefreshGrid(true);
}

function ShowPrivateVendors() {
    _public = false;

    $("#vendorsTabBar").find(".active").removeClass("active");
    $("#tabButtonPrivateVendors").addClass("active");
    $(".myVendorsStuff").show();
    $(".extra-note").hide();

    var grid = $("#vendor-grid").data("kendoGrid");
    if (grid) {
        grid.hideColumn(1);
    }

    ShowVendorType(2);
}

function ShowVendorType(vendorType) {
    _currentVendorType = vendorType;

    $("#partSourceTabBar").find(".active").removeClass("active");

    $("#partSourceTabBar").find(".vendor-" + vendorType).addClass("active");

    RefreshGrid(true);
}

function ShowRepairFacility() {
    ShowExtraType(8, "tabButtonRepairFacility");
}

function ShowAlternateIdentity() {
    ShowExtraType(3, "tabButtonAlternateIdentity");
}

function ShowExtraType(typeID, tabID) {
    _currentVendorType = typeID;
    _public = false;
    RefreshGrid(true);
    $(".myVendorsStuff").hide();
    $(".extra-note").hide();
    $("#vendorsTabBar").find(".active").removeClass("active");
    $("#" + tabID).addClass("active");

    var grid = $("#vendor-grid").data("kendoGrid");
    if (grid) {
        grid.hideColumn(1);
    }
}

$(document).ready(function () {

    $(".btnCreateNew").click(function () {
        ClearVendorForm();
    });

    $("#btnSearch").click(function () {
        RefreshGrid(true);
    });

    $("#SearchText").on('keydown', function (event) {
        // Check for Enter key or Esc key
        if (event.keyCode === 27) {
            $("#SearchText").val("");
        }

        if (event.keyCode === 13 || event.keyCode === 27) {
            $("#btnSearch").click();
            event.preventDefault(); // Prevent the default action
        }
    });

    $(".btnSave").click(function () {

        if(TrimSpaces($("#txtCompanyName").val())=='')
        {
            ShowMessage(_pageStrings.EnterCompanyName);
            return false;
        }

        var vm = VendorToVM();

        $.ajax({
            type: 'POST',
            url: '/Vendors/SaveVendor',
            data: {
                userID: _userID,
                loginID: _loginID,
                vendorVM: vm
            },
            success: function(data) {
                if (data.Success == true) {
                    LoadVendorIntoForm(data.Vendor);
                    ShowMessage(_pageStrings.VendorInformationSaved);
                    RefreshGrid(false);
                } else {
                    ShowMessage(data.ErrorMessage);
                }
            },
            error: function(error) {
                // Handle the error scenario
                console.log(error);
            }
        });
    });

    $(".btnDelete").click(function () {
        if((_currentVendorID||0)==0)
        {
            alert(_pageStrings.VendorDeleteAlertMessage);
            return;
        }
        if (confirm(_pageStrings.VendorDeleteConfirmMessage) == true) {
            $.getJSON('/Vendors/DeleteVendor', { userID: _userID, loginID: _loginID, vendorID: _currentVendorID }, function (data) {
                if (data.Success == true) {
                    ClearVendorForm();
                    RefreshGrid(true);
                    ShowMessage(_pageStrings.VendorDeleted);

                } else {
                    ShowMessage(data.ErrorMessage);
                }
            });
        }
    });

    CheckIfRepairFacilitySelected();
    ShowPublicVendors();
    ShowVendorType(2);
});

function RefreshGrid(hideForm) {
    if (hideForm === true) {
        HideVendorForm();
        _currentVendorID = 0;
    }

    var grid = $("#vendor-grid").data("kendoGrid");

    if (grid) {
        grid.dataSource.read();
        grid.refresh();

        grid.dataSource.page(1);
    }
}

function GetVendorsListParameters()
{
    var result = {
        loginID: _loginID
        , typeID: _currentVendorType
        , publicVendors: _public
        , filterText: $("#SearchText").val()
    };
    return result
}

function onDataBound(arg) {
    $(".vendor-checkbox").change(function() {
        var id = $(this).attr("data-id");

        $.getJSON('/Vendors/ToggleVendorSelection', { userID: _userID, loginID: _loginID, vendorID: id }, function () {
        });
    });

    // Wire up hilighting the row when hovering.
    var row = '';

    $("#vendor-grid tbody tr").hover(
        function () {
            row = $(this).closest("tr");
            row.toggleClass("k-state-hover");
        }
    );

    $("#vendor-grid tbody tr").click(
        function () {
            var vendorID = row.find("td").first().html();
            loadForEditing(vendorID);

            $("#vendor-grid").find(".k-state-selected").removeClass("k-state-selected");
            row.addClass("k-state-selected");
        }
    );

    if (_currentVendorID > 0) {
        HilightRow(_currentVendorID);
    }
}

function HilightRow(vendorID) {
    var grid = $("#vendor-grid").data("kendoGrid");
    var rows = grid.tbody.find("tr");

    rows.each(function (index, row) {
        var dataItem = grid.dataItem(row);
        if (dataItem.ID === vendorID) {
            $(row).addClass("k-state-selected");
            return false; // Break out of the each loop once the row is found
        }
    });
}