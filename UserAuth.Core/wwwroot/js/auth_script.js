$(document).ready(function () {
    var countdownTime = window.CountdownTime;
    function StartTimerCountDown() {
        var timerInterval = setInterval(function () {
            var minutes = Math.floor(countdownTime / 60);
            var seconds = countdownTime % 60;

            // Format the time as MM:SS
            var formattedTime = minutes.toString().padStart(2, '0') + ":" + seconds.toString().padStart(2, '0');
            $('#countdown-timer').text(formattedTime);

            if (countdownTime <= 0) {
                clearInterval(timerInterval);
                
                $('#countdown-timer').text("Time's up!");

                var Email = $("#EmailInputHidden").val();

                $.ajax({
                    url: '/Home/Delete2FaAuth',
                    type: 'POST',
                    data: {Email : Email},
                    success: function() {
                        toastr.error("Time out! try again",{ timeOut: 6000 });
                    },
                    error: function(error) {
                        console.error('Error delete user user auth token details:', error);
                    }
                });
            }

            countdownTime--;
        }, 1000);
    }
    StartTimerCountDown();
});