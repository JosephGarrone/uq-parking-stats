var express = require('express');
var mysql = require('mysql');
var router = express.Router();
var conn = mysql.createConnection({
    host: 'localhost',
    user: 'uq_parking',
    password: 'uq_parking',
    database: 'uq_parking'
})
conn.connect();

/* GET carparks */
router.get('/carparks', function( req, res, next) {
    conn.query('SELECT * FROM `car_parks`', function(err, rows, fields) {
        res.send(rows);
    });
});

/* GET data */
router.get('/:carpark/:date', function (req, res, next) {
    // Build query
    var qry, qryParams;
    if (req.params.carpark == "all") {
        
        qry = 'SELECT SUM(a.avg_available) AS available, a.`time` ' +
                'FROM ( ' +
                '    SELECT ROUND(AVG(available), 0) AS avg_available, FROM_UNIXTIME(((((UNIX_TIMESTAMP(`time`)+30) DIV 60) * 60) DIV 300) * 300) as `time` ' +
                '    FROM car_park_info ' +
                '    WHERE DATE(CONVERT_TZ(`time`, "+00:00", "+10:00")) = STR_TO_DATE(?, "%Y-%m-%d") ' +
                '    GROUP BY car_park, (((TIME_TO_SEC(`time`)+30) DIV 60) * 60) DIV 300 ' +
                ') AS a ' +
                'GROUP BY a.`time`';
        qryParams = [req.params.date];
        
    } else if (req.params.carpark == "casual" || req.params.carpark == "permit") {
        
        qry = 'SELECT SUM(a.avg_available) AS available, a.`time` ' +
                'FROM ( ' +
                '    SELECT ROUND(AVG(available), 0) AS avg_available, FROM_UNIXTIME(((((UNIX_TIMESTAMP(`time`)+30) DIV 60) * 60) DIV 300) * 300) as `time` ' +
                '    FROM car_park_info ' +
                '    LEFT JOIN car_parks ON (car_parks.id = car_park_info.car_park) ' +
                '    WHERE DATE(CONVERT_TZ(`time`, "+00:00", "+10:00")) = STR_TO_DATE(?, "%Y-%m-%d") ' +
                '    AND `car_parks`.`casual` = ? ' +
                '    GROUP BY car_park, (((TIME_TO_SEC(`time`)+30) DIV 60) * 60) DIV 300 ' +
                ') AS a ' +
                'GROUP BY a.`time`';
        
        if (req.params.carpark == "casual") {
            qryParams = [req.params.date, 1];
        } else {
            qryParams = [req.params.date, 0];
        }
        
    } else {
        qry = 'SELECT `available`, `time` ' +
                'FROM `car_park_info`' +
                'WHERE `car_park` = ? AND DATE(CONVERT_TZ(`time`, "+00:00", "+10:00")) = STR_TO_DATE(?, "%Y-%m-%d")' +
                'GROUP BY UNIX_TIMESTAMP(`time`) DIV 300;'
        
        qryParams = [req.params.date, req.params.carpark];
        
    }
    
    // Execute
    conn.query(qry, qryParams, function(err, rows, fields) {
        res.send(rows);
    });
})

module.exports = router;
