# Electricity sensors

This repository contains everything related to the development of **Atmospheric Electricity Sensors** for the ReACT team of the National Observatory of Athens. Specifically, two such sensors are designed to be suitable for weather balloon deployments:

- a Miniature FieldMill Electrometer (MiniMill)
- a Space Charge Sensor

Full description of the sensors is included in the [Wiki](https://github.com/NOA-ReACT/electricity-sensors/wiki) page.

## Index

In this repository you can find:

- `decoder`: A library, CMD tool and UI tool for decoding XDATA packages transmitted by the sensors through a GRAW radiosonde. Written in C#.
- `firmware`: Arduino sketches for the microcontrollers of the two sensors.
- `pcb`: Board designs and schematics for both sensors.
- `specs`: Sensor technical specifications.

## Acknowlegements
This research was supported by D-TECT (Grant Agreement 725698) funded by the European Research Council (ERC) under the European Union's Horizon 2020 research and innovation programme.

## Blame

- **Team lead:** Lilly Daskalopoulou <vdaskalop@noa.gr> @ModusElectrificus
- **Hardware Engineering**: Vasilis Spanakis-Misirlis <vspanakis@noa.gr>
- **Consulting**: Thanasis Georgiou <ageorgiou@noa.gr> @thgeorgiou

Fieldmill firmware uses code from the [ElectronicCats/mpu6050](https://github.com/ElectronicCats/mpu6050) library, available under the MIT license.
