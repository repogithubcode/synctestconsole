function wireUpPaintCalculations() {
    // Update the calculated paint values whenever the input changes.
    $("#TextBoxPaintTime").on('input', function () {
        calculatePaintValues();
    });

    // When editing of the paint time is done, make sure the input is a valid float
    $("#TextBoxPaintTime").change(function () {
        // If the paint popup is showing, use those controls that start with Popup_
        var prefix = "";

        if ($('#PaintPopup').is(':visible')) {
            prefix = "Popup_";
        }

        var paintHours = getPaintBaseTime(prefix);

        // Update the input Paint Hours textbox, makeing sure it's a float
        $("#" + prefix + "TextBoxPaintTime").val(roundNumber(paintHours));

        calculatePaintValues();
    });

    $("#DropDownListPaintType").change(function () {
        calculatePaintValues();
    });

    $("#DropDownListOverlap").change(function () {
        calculatePaintValues();
    });
}

function refreshPanelTypeWrapper() 
{
    // If the paint popup is showing, use those controls that start with Popup_
    var prefix = "";

    if ($('#PaintPopup').is(':visible')) {
        prefix = "Popup_";
    }

    var paintTime = $("#" + prefix + "TextBoxPaintTime").val();

    if ($("#" + prefix + "TextBoxPaintTime").is(':visible') && paintTime > 0) {
        $(".panel-type-wrapper").show();
    }
    else {
        $(".panel-type-wrapper").hide();
    }
}

function calculatePaintValues() {

    console.debug('calculatePaintValues');
    // If the paint popup is showing, use those controls that start with Popup_
    var prefix = "";

    if ($('#PaintPopup').is(':visible')) {
        prefix = "Popup_";
    }

    // The Panel Type drop down should only be visible for a Replace or Refinish operation
    refreshPanelTypeWrapper();
    var panelType = "";

    var operationType = $("#ManualEntry_Details_OperationType").val();
    if (operationType == "Replace" || operationType == "Refinish")
    {
        panelType = $("#" + prefix + "DropDownListOverlap").val();
    }

    var paintHours = getPaintBaseTime(prefix);
    var paintType = getPaintType(prefix);

    var blendGain = paintGain_Blend;
    if (paintType == 18 || paintType == 29)
    {
        blendGain = paintGain_TwoThreeBlend;
    }

    if (!panelType)
    {
        panelType = "";
    }        

    // Calculate blend before the adjacent deduction
    if (isNotLocked(prefix + "CheckBoxLockBlend")) {
        var blendTime = roundNumber(paintHours * blendGain);
        $("#" + prefix + "TextboxBlendTime").val(parseFloat(blendTime));
    }

    // If Adjacent or NonAdjacent panel is selected, remove the deduction from the base paint hours
    if (panelType == "Adjacent") {
        paintHours = paintHours - paintDeduction_Adjacent;
    }
    else if (panelType == "NonAdjacent") {
        paintHours = paintHours - paintDeduction_NonAdjacent;
    }

    if (isNotLocked(prefix + "CheckBoxLockEdging")) {
        if (paintHours >= paintGain_EdgingMin) {
            $("#" + prefix + "TextboxEdgingTime").val(parseFloat(0.5));
        }
    }

    if (isNotLocked(prefix + "CheckBoxLockUnderside")) {
        $("#" + prefix + "TextboxUndersideTime").val(roundNumber(parseFloat(paintHours * paintGain_Underside)));
        ShouldIncludeUnderside();
    }

    if (isNotLocked(prefix + "CheckBoxLockClearcoat")) {
        var singleStageMult = 1;
        if (paintType == 16)
        {
            singleStageMult = 0;
        }

        if (panelType == "First" || panelType == "")
        {
            $("#" + prefix + "TextboxClearcoatTime").val(roundNumber(paintHours * paintGain_ClearCoatMajor * singleStageMult));
        }
        else if (panelType == "Adjacent" || panelType == "NonAdjacent")
        {
            $("#" + prefix + "TextboxClearcoatTime").val(roundNumber(paintHours * paintGain_ClearCoatNonAdj * singleStageMult));
        }

        if (paintHours != 0 && paintType != 16)
        {
            $("#" + prefix + "CheckBoxIncludeClearcoat").prop('checked', true);
        }
        else
        {
            $("#" + prefix + "CheckBoxIncludeClearcoat").prop('checked', false);
        }

        if (paintType == 0)
        {
            $("#" + prefix + "CheckBoxIncludeClearcoat").prop('checked', false);
        }
    }

    // Calculations for 3 stage or 2 tone
    if (paintType == 18 || paintType == 29) {
        $("#" + prefix + "toneStageContainer").show();

        if (isNotLocked(prefix + "CheckBoxLockAllowance")) {
            var toneStageMult = 0;

            // 2 tone
            if (paintType == 29) {
                toneStageMult = paintGain_2ToneMajor;

                if (panelType == "NonAdjacent") {
                    toneStageMult = paintGain_2ToneNonAdjacent;
                }
            }
            // or 3 stage
            else if (paintType == 18) {
                toneStageMult = paintGain_3StageMajor;

                if (panelType == "NonAdjacent") {
                    toneStageMult = paintGain_3StageNonAdjacent;
                }
            }

            var allowanceTime = roundNumber(paintHours * toneStageMult);
            $("#" + prefix + "TextboxAllowanceTime").val(allowanceTime);

            if (allowanceTime > 0)
            {
                $("#" + prefix + "CheckBoxIncludeAllowance").prop("checked", true);
            }
        }

        disableControl(prefix + "TextboxClearcoatTime");
        disableControl(prefix + "CheckBoxIncludeClearcoat");
        disableControl(prefix + "CheckBoxLockClearcoat");
    } else {
        $("#" + prefix + "toneStageContainer").hide();

        enableControl(prefix + "TextboxClearcoatTime");
        enableControl(prefix + "CheckBoxIncludeClearcoat");
        enableControl(prefix + "CheckBoxLockClearcoat");
    }
}

function disableControl(controlID) {
    $("#" + controlID).prop("disabled", true);
    $("#" + controlID).css("color", "grey");
    $("#" + controlID).css("opacity", "0.5");
}

function enableControl(controlID) {
    $("#" + controlID).prop("disabled", false);
    $("#" + controlID).css("color", "black");
    $("#" + controlID).css("opacity", "1.5");
}

function roundNumber(number)
{
    return Math.round(number * 10) / 10
}

function isNotLocked(controlName) {
    var isChecked = $("#" + controlName).prop("checked");
    return !isChecked;
}

function getPaintType(prefix) {
    return $("#" + prefix + "DropDownListPaintType").val();
}

function getPaintBaseTime(prefix) {
    var paintHours = getFloat($("#" + prefix + "TextBoxPaintTime").val());
    if (isNaN(paintHours)) {
        paintHours = 0;
    }

    return paintHours;
}

function getFloat(input) {
    input = input.replace(/[^0-9.-]/g, '');
    var returnVal = parseFloat(input);
    if (returnVal) {
        return returnVal;
    }

    return 0;
}

var currentClickedRow;
function GetClickedRow(clickedRow)
{
    currentClickedRow = clickedRow;
}

function ShouldIncludeUnderside() {

    // If the paint popup is showing, use those controls that start with Popup_
    var prefix = "";

    if ($('#PaintPopup').is(':visible')) {
        prefix = "Popup_";
    }

    if (prefix == "Popup_") {

        var sectionName = '';
        var partDescription = '';

        if ($("#PartsAndLaborContainer").is(":visible")) {
            sectionName = $("#sectionSelect option:selected").text();
            partDescription = $(currentClickedRow).closest("tr").find("td:nth-child(4)").html();
        }
        else
        {
            // Get the selected item and call the ClickedItem event
            var rowSection = $("#sections-grid").find(".k-state-selected").first();
            sectionName = rowSection.find("td:nth-child(2)").html();

            // Get the selected item and call the ClickedItem event
            var rowParts = $("#parts-grid").find(".k-state-selected").first();
            partDescription = rowParts.find("td:nth-child(2)").text();
        }

        // sectionName
        if ($.trim(sectionName) == '') {
            sectionName = $("#sectionSelect option:selected").text();
        }

        // partDescription
        if ($.trim(partDescription) == '') {
            partDescription = _partDescription;
        }

        sectionName = $.trim(sectionName).toLowerCase();
        console.log('isNotLocked--> sectionName : ' + $.trim(sectionName));

        partDescription = $.trim(partDescription).toLowerCase();
        console.log('isNotLocked--> partDescription : ' + $.trim(partDescription));

        // Lift Gate
        var sectionLiftGate = "Liftgate \\ Sheet Metal";
        var partDescriptionLiftGate = "liftgate shell";

        // Trunk Lid (Luggage Lid)
        var sectionTrunkLuggage = "Luggage Lid";
        var partDescriptionTrunkLuggage = "luggage Lid";

        // Hood Panel
        var sectionHood = "Hood";
        var partDescriptionHood = "hood panel(alum)";

        // Tailgate Shell
        var sectionTailgateMultifunction = "Tailgate \\ Multifunction";
        var sectionPickupBedSheetMetal = "Pickup Bed \\ Sheet Metal";
        var sectionTailgateStandard = "Tailgate \\ Standard";
        var partDescriptionTailgate = "tailgate";

        // Back Door
        var sectionBackDoor = "Back Door \\ Sheet Metal";
        var partDescriptionBackDoor = "back door shell";

        // Front Door
        var sectionFrontDoor = "Front Door \\ Sheet Metal";
        var partDescriptionFrontDoor = "door shell";

        // Side Door
        var sectionSideDoor = "Side Door \\ Sheet Metal";
        var partDescriptionSideDoor = "door shell";

        // Rear Door
        var sectionRearDoor = "Rear Door \\ Sheet Metal";
        var partDescriptionRearDoor = "rear door shell";

        if (HeaderAction == 'Replace' &&
            (((sectionName.includes(sectionLiftGate.toLowerCase())) && (partDescription.includes(partDescriptionLiftGate.toLowerCase()))) ||
            ((sectionName.includes(sectionTrunkLuggage.toLowerCase())) && (partDescription.includes(partDescriptionTrunkLuggage.toLowerCase()))) ||
            ((sectionName.includes(sectionHood.toLowerCase())) && (partDescription.includes(partDescriptionHood.toLowerCase()))) ||

            ((sectionName.includes(sectionTailgateMultifunction.toLowerCase()) || sectionName.includes(sectionPickupBedSheetMetal.toLowerCase())
              || sectionName.includes(sectionTailgateStandard.toLowerCase()))
                && (partDescription.includes(partDescriptionTailgate.toLowerCase()))) ||

            ((sectionName.includes(sectionBackDoor.toLowerCase())) && (partDescription.includes(partDescriptionBackDoor.toLowerCase()))) ||
            ((sectionName.includes(sectionFrontDoor.toLowerCase())) && (partDescription.includes(partDescriptionFrontDoor.toLowerCase()))) ||
            ((sectionName.includes(sectionSideDoor.toLowerCase())) && (partDescription.includes(partDescriptionSideDoor.toLowerCase()))) ||
            ((sectionName.includes(sectionRearDoor.toLowerCase())) && (partDescription.includes(partDescriptionRearDoor.toLowerCase()))))
           ) {

            // make CheckBoxIncludeUnderside true
            $("#" + prefix + "CheckBoxIncludeUnderside").prop('checked', true);
        }
        else {
            $("#" + prefix + "CheckBoxIncludeUnderside").prop('checked', false);
        }
    }
}