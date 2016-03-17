google.charts.load('current', { 'packages': ['corechart'] })
google.charts.setOnLoadCallback(function() {
    $('#car-park').change(drawChart);
    $('#graph-type').change(drawChart);
    $('#view-date').change(drawChart); 
    $('#refresh').click(drawChart);
    drawChart();
});

function drawChart() {
    var carPark = $('#car-park option:selected').text();
    var carParkId = $('#car-park').val();
    
    var date = $('#view-date').pickadate('picker').get('select', 'yyyy-mm-dd');
    
    var data = new google.visualization.DataTable();
    data.addColumn('datetime', 'Time');
    data.addColumn('number', carPark);
    
    $("#loader").removeClass("hide");
    var width = $("#graph").outerWidth();
    $("#graph").addClass("hide");
    
    $.ajax('/' + carParkId + '/' + date).done(function(json) {
        if (json.length == 0) {
            $("#graph").addClass("hide");
            $("#no-data").removeClass("hide");
            return;
        }
        
        for (var i = 0; i < json.length; i++) {
            data.addRow([new Date(json[i].time), json[i].available])
        }
        
        var start = new Date(date);
        start.setHours(0, 0, 0, 0);
        var end = new Date(date);
        end.setHours(23, 59, 59, 999);
        console.log(width);
        var options = {
            height: 450,
            width: width,
            chartArea: {
                width: '75%',
                height: '80%'
            },
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
                title: 'Available Parks'
            },
            hAxis: {
                titleTextStyle: { 
                    italic: false
                },
                title: 'Time',
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