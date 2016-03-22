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
    if (req.params.carpark == "all") {
        conn.query('SELECT SUM(a.available) AS `available`, `a`.`time` FROM (SELECT `available`, `time` FROM `car_park_info` WHERE DATE(CONVERT_TZ(`time`, "+00:00", "+10:00")) = STR_TO_DATE(?, "%Y-%m-%d") GROUP BY `car_park`, UNIX_TIMESTAMP(`time`) DIV 300) AS `a` GROUP BY `time`;', [req.params.date], function(err, rows, fields) {
            res.send(rows);
        });
    } else if (req.params.carpark == "casual") {
        conn.query('SELECT SUM(a.available) AS `available`, `a`.`time` FROM (SELECT `available`, `time` FROM `car_park_info` LEFT JOIN `car_parks` ON (`car_parks`.`id` = `car_park_info`.`car_park`) WHERE DATE(CONVERT_TZ(`time`, "+00:00", "+10:00")) = STR_TO_DATE(?, "%Y-%m-%d") AND `car_parks`.`casual` = 1 GROUP BY `car_park`, UNIX_TIMESTAMP(`time`) DIV 300) AS `a` GROUP BY `time`;', [req.params.date], function(err, rows, fields) {
            res.send(rows);
        });
    } else if (req.params.carpark == "permits") {
        conn.query('SELECT SUM(a.available) AS `available`, `a`.`time` FROM (SELECT `available`, `time` FROM `car_park_info` LEFT JOIN `car_parks` ON (`car_parks`.`id` = `car_park_info`.`car_park`) WHERE DATE(CONVERT_TZ(`time`, "+00:00", "+10:00")) = STR_TO_DATE(?, "%Y-%m-%d") AND `car_parks`.`casual` = 0 GROUP BY `car_park`, UNIX_TIMESTAMP(`time`) DIV 300) AS `a` GROUP BY `time`;', [req.params.date], function(err, rows, fields) {
            res.send(rows);
        });
    } else {
        conn.query('SELECT `available`, `time` FROM `car_park_info` WHERE `car_park` = ? AND DATE(CONVERT_TZ(`time`, "+00:00", "+10:00")) = STR_TO_DATE(?, "%Y-%m-%d") GROUP BY UNIX_TIMESTAMP(`time`) DIV 300;', [req.params.carpark, req.params.date], function(err, rows, fields) {
            res.send(rows);
        });
    }
})

module.exports = router;
