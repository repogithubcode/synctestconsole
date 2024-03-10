var _pdrUserID, _pdrEstimateID;
var _pdrAdditionalOperationsData;
var _isAddingAdditionalOperation = false;


$(document).ready(function () {
    $("#PDRAdditionalOperations_Container").click(function () {
        CloseAdditionalOperations();
    });

    $("#PDRAdditionalOperations_Container").find(".close-x").click(function () {
        CloseAdditionalOperations();
    });

    $("#PDRAdditionalOperationsType").change(function () {
        RefreshAditionalOperations($("#PDRAdditionalOperationsType").val());
    });
});

function CloseAdditionalOperations() {
    $("#PDRAdditionalOperations_Container").fadeOut();
}

function RefreshPDRSectionsData(panelID, keepSelection) {
    $.getJSON('/Estimate/GetSectionsForPDRPanel', {
          userID: _pdrUserID
        , estimateID: _pdrEstimateID
        , panelID: panelID
    }, function (data) {
        _pdrAdditionalOperationsData = data;

        if (data.Success === true) {
            var mainPanel = _pdrAdditionalOperationsData.PanelDetails;
            $("#additionalOperationsSection").text(mainPanel.PanelName);

            var ddl = $("#PDRAdditionalOperationsType");

            var selectedSection = ddl.val();

            ddl.empty();

            for (var i = 0; i < mainPanel.SectionDetails.length; i++) {
                ddl.append($('<option>', {
                    value: mainPanel.SectionDetails[i].Name,
                    text: mainPanel.SectionDetails[i].Name
                }));
            }

            if (keepSelection === true && selectedSection) {
                ddl.val(selectedSection);
                RefreshAditionalOperations(selectedSection);
            }
            else {
                ddl.val(mainPanel.SectionDetails[0].Name);
                RefreshAditionalOperations(mainPanel.SectionDetails[0].Name);
            }

            if (mainPanel.SectionDetails.length > 0) {
                $("#PDRAdditionalOperations_Container").fadeIn();
            }
            else {
                $("#PDRAdditionalOperations_Container").fadeOut();
            }
        }
        else {
            ShowUserMessage(data.ErrorMessage, true, 2500);
        }
    });
}

function RefreshAditionalOperations(sectionName) {
    var sectionDetails = GetSectionDetailsByName(_pdrAdditionalOperationsData.PanelDetails.SectionDetails, sectionName);

    if (sectionDetails) {
        var tableHtml = '<TABLE Width="100%" Border="0" style="margin: 0px;"><TR Class="TableHeader"><TH width="100">Action</TH><TH width="250">Description</TH><TH width="400">Comment</TH><TH width="100">Labor</TH><TH>Notes</TH></TR>';

        if (sectionDetails.Parts) {
            tableHtml += AddSectionToTable(_pdrAdditionalOperationsData.PanelDetails.PanelID, sectionDetails);
        }

        if (_pdrAdditionalOperationsData.AdjacentPanelDetails) {
            for (var adjacentIndex = 0; adjacentIndex < _pdrAdditionalOperationsData.AdjacentPanelDetails.length; adjacentIndex++) {
                var adjacentSection = GetSectionDetailsByName(_pdrAdditionalOperationsData.AdjacentPanelDetails[adjacentIndex].SectionDetails, sectionName);
                if (adjacentSection) {
                    tableHtml += '<tr><td colspan="10"><h2>' + _pdrAdditionalOperationsData.AdjacentPanelDetails[adjacentIndex].PanelName + '</h2></td></tr>';
                    tableHtml += AddSectionToTable(_pdrAdditionalOperationsData.AdjacentPanelDetails[adjacentIndex].PanelID, adjacentSection);
                }
            }
        }

        tableHtml += '</TABLE>';

        $('#PDRAdditionalOperations').html(tableHtml);

        $("#PDRAdditionalOperations_Container").fadeIn();
    }
}

function GetSectionDetailsByName(allSectionDetails, name) {
    for (i = 0; i < allSectionDetails.length; i++) {
        if (allSectionDetails[i].Name == name) {
            return allSectionDetails[i];
        }
    }

    return null;
}

function AddSectionToTable(panelID, sectionDetails) {
    var returnString = "";

    if (sectionDetails.Parts) {
        var TDString, tempPart, partSplit, TDString;
        var re = /N/gi;

        var lastComment = "";

        for (var i = 0; i < sectionDetails.Parts.length; i++) {

            var part = sectionDetails.Parts[i];

            if (sectionDetails.Parts[i].comment != lastComment) {
                var T = "";

                // Button IDs need to be unique, if there's no barcode, generate a random string
                var partBarcode = part.Barcode;
                if (!partBarcode || partBarcode.length <= 0) {
                    partBarcode = MakeRandomString(5);
                }

                lastComment = part.comment;

                tempPart = part.PartID;
                if (tempPart) {
                    partSplit = tempPart.split('.');
                    PartId = partSplit[0];
                }
                else {
                    PartId = "";
                }

                var T = '<TR class="OverPart">';
                if (Math.round(i / 2) == i / 2) {
                    TDString = '<TD Class="TableData">';
                }
                else {
                    TDString = '<TD Class="TableDataAlt">';
                }

                PartId = PartId.replace(re, '');


                // ID is the estimate line ID, if it's > 0 the part is already in the estimate
                if (part.ID > 0) {
                    T = T + TDString + '<INPUT Type="Button" id="Add' + partBarcode + '"  value="Remove" onclick="javascript:DeleteAdditionalOperation(' + part.ID + ');">';
                }
                else {
                    T = T + TDString + '<INPUT Type="Button" id="Add' + partBarcode + '"  value="Add" onclick="javascript:AddAdditionalOperation(' + sectionDetails.Header + ', ' + sectionDetails.Section + ', ' + i + ', ' + panelID + ', this);">';
                }
                T = T + '</TD>' + TDString + part.Description + '</TD>' + TDString + part.comment + '</TD>' + TDString;

                var laborTime = 0;

                if (part.OHTime > 0) { laborTime = part.OHTime }
                if (part.AddTime > 0) { laborTime = part.AddTime }
                if (part.AITime > 0) { laborTime = part.AITime }
                if (part.CATime > 0) { laborTime = part.CATime }
                if (part.AlignTime > 0) { laborTime = part.AlignTime }
                if (part.RITime > 0) { laborTime = part.RITime }
                if (part.RRTime > 0) { laborTime = part.RRTime }

                if (laborTime > 0) {
                    T = T + laborTime + '&nbsp;' + part.LaborName;
                }

                if (part.RefinishTime > 0) {
                    if (laborTime > 0) {
                        T = T + '<BR>';
                    }

                    T = T + part.RefinishTime + '&nbsp;' + part.PaintName;
                }

                T = T + '</TD>' + TDString + part.Notes + '</TD>';
                T = T + '</TR>';

                returnString += T;
            }
        }
    }

    return returnString;
}

function OpenAdditionalOperations(panelID) {
    RefreshPDRSectionsData(panelID, false);    
}

function DeleteAdditionalOperation(lineID) {
    var okToDelete = confirm('Are you sure you want to delete this line?');

    if (okToDelete) {
        $.getJSON("/Estimate/DeleteLineItem", {
              userID: _pdrUserID
            , estimateID: _pdrEstimateID
            , lineID: lineID
            , meMode: ''
        }, function (data) {
            RefreshHeaderInfo(estimateID);
            LoadEstimateLineItemList();
            RefreshPDRSectionsData(_pdrAdditionalOperationsData.PanelDetails.PanelID, true);
        });
    }
}

function AddAdditionalOperation(header, section, index, panelID, button) {

    $(button).css("opacity", 0.5);

    if (_isAddingAdditionalOperation === false) {
        _isAddingAdditionalOperation = true;

        $.getJSON("/Estimate/AddAdditionalOperation", {
            userID: _pdrUserID
            , estimateID: _pdrEstimateID
            , panelID: panelID
            , header: header
            , section: section
            , index: index
        }, function (data) {
            _isAddingAdditionalOperation = false;

            if (data.Success == true) {
                RefreshHeaderInfo(estimateID);
                LoadEstimateLineItemList();
                RefreshPDRSectionsData(_pdrAdditionalOperationsData.PanelDetails.PanelID, true);
            }
            else {
                ShowProgressMessage(data.ErrorMessage, 10000);
            }
        });
    }
}