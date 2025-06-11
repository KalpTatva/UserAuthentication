$(document).ready(function () {
    

    $(document).on('submit','#SendMessage',function(e){
        e.preventDefault();
        var formData = $(this).serialize();
        $.ajax({
            url: '/Dashboard/SendMessage',
            type: 'POST',
            data : formData,
            success: function(data) {
                if(data.success)
                {
                    toastr.success(data.message, 'Success', {timeOut:5000} );
                    fetchMessages()
                }
                else
                {
                    toastr.error(data.message, 'Error', {timeOut:5000} );
                }
            },
            error: function(error) {
                var message  = 'Error sending message:' + error;
                toastr.error(message, 'Error', {timeOut:5000} );

            }
        })
    });

});