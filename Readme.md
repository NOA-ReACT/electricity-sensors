# Electricity sensors

This repository contains everything related to the development of Atmospheric Electricity Sensors at the ReACT team of the National Observatory of Athens. Specifically, two such sensors are worked on, designed to be suitable for weather balloon deployments:

- Miniature Field-Mill Electrometer
- Space Charge Sensor

Full documentation is included in the Wiki page: https://github.com/NOA-ReACT/electricity-sensors/wiki.

## Index

In this repository you can find:

- `decoder`: A library, CMD tool and UI tool for decoding XDATA packages transmitted by the sensors through a GRAW radiosonde. Written in C#.
- `firmware`: Arduino sketches for the microcontrollers of the two sensors.
- `pcb`: Board designs in Eagle
- `MiniMill_specs.pdf`: Miniature Field-Mill Electrometer technical specifications
- `Charge_sensor_specs.pdf`: Space Charge Sensor technical specifications

## Acknowlegements
This research was supported by D-TECT (Grant Agreement 725698) funded by the European Research Council (ERC) under the European Union's Horizon 2020 research and innovation programme.

## Blame

- **Team lead:** Lilly Daskalopoulou <vdaskalop@noa.gr> @ModusElectrificus
- **Hardware Engineering**: Vasilis Spanakis-Misirlis <vspanakis@noa.gr>
- **Consulting**: Thanasis Georgiou <ageorgiou@noa.gr> @thgeorgiou

Field mill firmware uses code from the [ElectronicCats/mpu6050](https://github.com/ElectronicCats/mpu6050) library, available under the MIT license.
