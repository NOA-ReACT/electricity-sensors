# Electricity sensors

This repository contains everything related to the development of Atmospheric Electricity Sensors at ReACT. Specifically, two such sensors are worked on, designed to be suitable for weather balloon deployments:

* Miniature Field-Mill
* Space Charge Sensor

Documentation is not included in this repository. You should refer to the [project's book on ReACT knowledge base](https://react.space.noa.gr/kb/books/electricity-sensors).

## Index

In this repository you can find:

* `decoder`: A library, CMD tool and UI tool for decoding XDATA packages transmitted by the sensors through a GRAW radiosonde. Written in C#.
* `firmware`: Arduino sketches for the microcontrollers of the two sensors.

## Blame

* **Team lead:** Lilly Daskalopoulou <vdaskalop@noa.gr> @ModusElectrificus
* **Hardware Engineering**: Vasilis Spanakis-Misirlis <vspanakis@noa.gr>
* **Consulting**: Thanasis Georgiou <ageorgiou@noa.gr> @thgeorgiou