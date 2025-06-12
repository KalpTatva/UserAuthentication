$(document).ready(function () {
    // Extract digit from the URL
    const urlParams = new URLSearchParams(window.location.search);
    const messageTo = urlParams.get('messageTo'); // This will get the value of 'messageTo'

    // console.log('Extracted digit:', messageTo); // Log the digit to verify


    function fetchMessagesForAdmin(messageTo)
    {
        $.ajax({
            url: '/Dashboard/ReciveMessageAdmin',
            type: 'GET',
            data: {messageTo : messageTo},
            success: function (data) {
                if(data.success == false)
                {
                    toastr.error(data.message, 'Error', { timeOut: 5000 });
                }
                console.log(data);
                $("#ReciveMessage").html(data);

            },
            error: function (error) {
                var message = 'Error reciving message:' + error;
                toastr.error(message, 'Error', { timeOut: 5000 });
            }
        })
    }

    $(document).on('submit', '#SendMessage', function (e) {
        e.preventDefault();
        var formData = $(this).serialize();
        $.ajax({
            url: '/Dashboard/SendMessage',
            type: 'POST',
            data: formData,
            success: function (data) {
                if (data.success) {
                    toastr.success(data.message, 'Success', { timeOut: 5000 });
                    fetchMessagesForAdmin(messageTo);
                } else {
                    toastr.error(data.message, 'Error', { timeOut: 5000 });
                }
            },
            error: function (error) {
                var message = 'Error sending message:' + error;
                toastr.error(message, 'Error', { timeOut: 5000 });
            }
        });
    });

    fetchMessagesForAdmin(messageTo);
});
