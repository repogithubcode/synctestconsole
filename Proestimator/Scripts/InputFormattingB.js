$(document).ready(function ($) {
     
    UpdateFormattedText();
    
    $(".datepicker").datepicker();

    // Change a text input to a formatted currency value like $50.00 
    $(document).on('change', '.currency', function () {
        var currentValue = $(this).val().replace(/[^0-9.-]/g, '');
        if (currentValue != '')
        {
            $(this).val(parseFloat(currentValue, 10).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString());
        }
    });

    // Text boxes with a class alphanumericOnly only allow letters and numbers
    $("input.alphanumericOnly").bind("keyup", function (e) {
        this.value = this.value.replace(/[^a-zA-Z0-9]/g, '');
    });

    $(".email-address").each(function (index) {
        var emailControl = $(this).attr('id');
        ValidateEmail(emailControl);
    });

    // Text boxes with a class email-address to validate Email
    $("input.email-address").bind("blur", function (e) {
        var emailControl = $(this).attr('id');
        ValidateEmail(emailControl);
    });
});

// This calls the change event on inputs that need formatting after loading.  This should be called by forms after they load data from the database that
// might not be formatted for display.
function UpdateFormattedText()
{
    $(".phone").unmask().mask("(999) 999-9999");

    $(".currency").change();
    $(".phone").change();
}


function ValidateEmail(emailControl) {
    var expr = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;

    $("#" + emailControl).css("border", "");

    // Email regex 
    var email = document.getElementById(emailControl).value;
    if (email != '') {
        if (!expr.test(email)) {
            $("#" + emailControl).css("border", "1px solid #CD0A0A");
        }
    }
}

function IsEmailAddressValid(emailControl) {
    var expr = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;

    // Email regex 
    var email = $('#' + emailControl).val();
    if (email != '') {
        if (!expr.test(email)) {
            return false;
        }
        else {
            return true;
        }
    }
}