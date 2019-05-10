var apiBase = "https://api.uqparking.com/";
var date = new Date();

google.charts.load('current', { 'packages': ['corechart'] })
google.charts.setOnLoadCallback(function() {
    $('#car-park').change(drawChart);
    $('#graph-type').change(drawChart);
    $('#view-date').change(drawChart); 
    $('#refresh').click(drawChart);
    drawChart();
});

// Initialise materialise
M.AutoInit();
$('.datepicker').datepicker({
    defaultDate: new Date(),
    autoClose: true, // Close upon selecting a date,
    onClose: function() {
        date = this.date;
    }
});

function drawChart() {
    var carPark = $('#car-park option:selected').text();
    var carParkId = $('#car-park').val();
    
    var data = new google.visualization.DataTable();
    data.addColumn('datetime', 'Time');
    data.addColumn('number', carPark);
    
    $("#loader").removeClass("hide");
    var width = $("#graph").outerWidth();
    var height = 450;
    $("#graph").addClass("hide");
    
    month = date.getMonth() + 1;
    if (month < 10) {
        month = "0" + month;
    }

    day = date.getDate();
    if (day < 10) {
        day = "0" + day;
    }

    $.ajax(apiBase + 'data/' + carParkId + '/' + date.getFullYear() + '/' + month + '/' + day).done(function(json) {
        if (json.length == 0) {
            $("#graph").addClass("hide");
            $("#no-data").removeClass("hide");
            return;
        }
        
        for (var i = 0; i < json.length; i++) {
            data.addRow([new Date(json[i].time), json[i].available])
        }
        
        var hTitle = "Time";
        var vTitle = "Available Parks";
        var chartArea = {
            left: 100,
            right: 95,
            top: 10,
            bottom: 70,
            width: '80%',
            height: '80%'
        };
        
        if (width < height) {
            hTitle = null;
            vTitle = null;
            var chartArea = {
                left: 45,
                top: 10,
                bottom: 35,
                right: 10,
                width: '75%',
                height: '80%'
            };
        }
        
        var start = new Date(date);
        start.setHours(0, 0, 0, 0);
        var end = new Date(date);
        end.setHours(23, 59, 59, 999);
        
        var options = {
            height: height,
            width: width,
            chartArea: chartArea,
            legend: {
                position: 'none'
            },
            vAxis: {
                titleTextStyle: { 
                    italic: false
                },
                viewWindow: {
                    min: 0
                },
                title: vTitle,
            },
            hAxis: {
                slantedText: true,
                titleTextStyle: { 
                    italic: false
                },
                title: hTitle,
                format: 'HH:mm',
                viewWindow: {
                    min: start,
                    max: end
                },
                gridlines: {
                    count: 24
                },
                minorGridlines: {
                    count: 1
                }
            }
        };

        var chart = new google.visualization.LineChart(document.getElementById('graph'));

        chart.draw(data, options);
        
        $("#graph").removeClass("hide");
        $("#no-data").addClass("hide");
    }).fail(function() {
        $("#graph").addClass("hide");
        $("#no-data").removeClass("hide");
    }).always(function() {
        $("#loader").addClass("hide");
    })
}

function loadCarparks() {
    $.ajax(apiBase + 'carparks').done(function(json) {
        for (var i = 0; i < json.length; i++) {
            $("#car-park").append($('<option>', {
                value: json[i]['Id'],
                text: json[i]['Id'] + ' - ' + json[i]['Name']
            }));
        }
    }).fail(function() {

    }).always(function() {

    })
}

loadCarparks();