$(document).ready(function () {
    function fetchLogs() {
        $('.loader').show();
        $.ajax({
            url: '/Dashboard/LogsDetails',
            type: 'GET',
            success: function (data) {
                $('#logContent').html(data);
                $('.loader').hide();
            },
            error: function (xhr, status, error) {
                console.error('Error fetching logs : ', error);
                $('#logContent').html('<p class="text-danger">Failed to load logs.</p>');
                $('.loader').hide();
            }
        });
    }

    fetchLogs();
})