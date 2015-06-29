# KinectSandbox

KinectSandbox is a *.Net* application using a Kinect 1st generation device to create an augmented reality map representing elevation.

Image generated can then be displayed with a video projector on the ground.

This application was created after looking at this video: [İnteraktif Topoğrafya Haritas] (https://www.youtube.com/watch?v=PHz4LkeIeC0) and inspired by related software: [SARndbox] (http://idav.ucdavis.edu/~okreylos/ResDev/SARndbox/)

User interface give access to all settings required to calibrate sensor and produce a good quality image output.

![User Interface 1](https://github.com/jvinel/KinectSandbox/blob/master/snapshots/snapshot1.png)
![User Interface 2](https://github.com/jvinel/KinectSandbox/blob/master/snapshots/snapshot2.png)

All level detections and colorations are performed using an image processing framework [AForge.net] (http://www.aforgenet.com/), and therefore doesn't required any specific graphic card.

Coloring is performed by using a jpeg or png image:

* ![Gradient 1](https://github.com/jvinel/KinectSandbox/blob/master/snapshots/gradient1.jpg)
* ![Gradient 3](https://github.com/jvinel/KinectSandbox/blob/master/snapshots/gradient3.jpg)

Additional colour gradient can be found at [cpt-city](http://soliton.vm.bytemark.co.uk/pub/cpt-city/)
#License

The KinectSandbox project is covered under a GPL v3 license. The licensing criteria are listed below, as well as at the top of each source file in the repo.

```
KinectSandbox is an augmented reality terrain mapping application.
Copyright (C) 2015 Julien Vinel jvinel@gmail.com

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
```

#More information 

Information about the kinectSandbox project can be found at [KinectSandbox Wiki] (https://github.com/jvinel/KinectSandbox/wiki)

You can also contact me on Twitter: http://twitter.com/jvinel




