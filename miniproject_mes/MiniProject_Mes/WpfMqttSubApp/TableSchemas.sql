CREATE TABLE `smarthome`.`sensing_datas` (
	`idx` BININT NOT NULL AUTO_INCREMENT,
  `Light` INT NOT NULL,
  `Rain` INT NOT NULL,
  `Temp` FLOAT NOT NULL,
  `Humid` FLOAT NOT NULL,
  `Fan` VARCHAR(3) NOT NULL,
  `Vulernability` VARCHAR(3) NOT NULL,
  `Real_Light` VARCHAR(3) NOT NULL,
  `ChaimBell` VARCHAR(3) NOT NULL,
  `Sensing_date` DATETIME NOT NULL,
  PRIMARY KEY(`idx`);
