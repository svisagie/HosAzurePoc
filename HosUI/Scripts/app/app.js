$(document).ready(function () {
    worker();
});

var getData = function (callback) {
    $.ajax({
        url: '/dashboard/data'
    })
    .success(function (data) {
        callback(true, data);
    })
    .error(function () {
        callback(false, undefined);
    });
};

var render = function (data) {
    var workstates
           = "<tr>"
           + "<th>DriverWorkStateId</th>"
           + "<th>DriverId</th>"
           + "<th>DriverName</th>"
           + "<th>WorkStateId</th>"
           + "<th>Timestamp</th>"
           + "</tr>";

    for (var i = 0; i < data.Workstates.length; i++) {
        workstates
            += "<tr>"
             + "<td>" + data.Workstates[i].DriverWorkStateId + "</td>"
             + "<td>" + data.Workstates[i].DriverId + "</td>"
             + "<td>" + data.Workstates[i].DriverName + "</td>"
             + "<td>" + data.Workstates[i].WorkStateId + "</td>"
             + "<td>" + data.Workstates[i].Timestamp + "</td>"
             + "</tr>";
    }
    
    var summaries
           = "<tr>"
            +"<th>DriverId</th>"
            +"<th>DriverName</th>"
            +"<th>WorkStateId</th>"
            +"<th>Total Seconds</th>"
            +"</tr>";

    for (var i = 0; i < data.Summaries.length; i++) {
        summaries
            += "<tr>"
                + "<td>" + data.Summaries[i].DriverId + "</td>"
                + "<td>" + data.Summaries[i].DriverName + "</td>"
                + "<td>" + data.Summaries[i].WorkStateId + "</td>"
                + "<td>" + data.Summaries[i].TotalSeconds + "</td>"
                +"</tr>";
    }

    $('#workStates').html(workstates);

    $('#summaries').html(summaries);
};

var worker = function () {
    getData(result);
};

var result = function (result, data) {
    if (result) {
        render(data);
    }
    worker();
};