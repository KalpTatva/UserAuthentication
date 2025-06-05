
$(document).ready(function () {
    let count = false;
    let count2 = false;
    let count3 = false;
    
    function togglePassword(eyeOpen, eyeClose, input, countState) {
        countState = !countState;
        if (countState) {
            $(eyeOpen).addClass("active").removeClass("inactive");
            $(eyeClose).removeClass("active").addClass("inactive");
            $(input).attr("type", "text");
        } else {
            $(eyeOpen).addClass("inactive").removeClass("active");
            $(eyeClose).removeClass("inactive").addClass("active");
            $(input).attr("type", "password");
        }

        return countState;
    }

    $(".openeye, .closeeye").on("click", function () {
        count = togglePassword(".openeye", ".closeeye", "#exampleInputPassword", count);
    });

    $(".openeye1, .closeeye1").on("click", function () {
        count2 = togglePassword(".openeye1", ".closeeye1", "#exampleInputPassword2", count2);
    });
    $(".openeye2, .closeeye2").on("click", function () {
        count3 = togglePassword(".openeye2", ".closeeye2", "#exampleInputPassword3", count3);
    });
    


    var LogoutModal = new bootstrap.Modal(document.getElementById('LogoutModal'), {
        keyboard: false,
        backdrop: 'static'
    });

    $("#LogoutButton").on("click", function (e) {
        e.preventDefault();
        LogoutModal.show();
    });
});
