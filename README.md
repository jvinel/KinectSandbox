# KinectSandbox

KinectSandbox is a *.Net* application using a Kinect 1st generation device to create an augmented reality map representing elevation.

Image generated can then be displayed with a video projector on the ground.

This application was created after looking at this video: [İnteraktif Topoğrafya Haritas] (https://www.youtube.com/watch?v=PHz4LkeIeC0) and inspired by related software: [SARndbox] (http://idav.ucdavis.edu/~okreylos/ResDev/SARndbox/)

User interface provided give access to all settings required to calibrate sensor and produce a good quality image output.

![User Interface 1](https://github.com/jvinel/KinectSandbox/blob/master/snapshots/snapshot1.png)
![User Interface 2](https://github.com/jvinel/KinectSandbox/blob/master/snapshots/snapshot2.png)

All level detections and colorations are performed using an image processing framework [AForge.net] (http://www.aforgenet.com/), and therefore doesn't required any specific graphic card.

Coloring is performed by using a 256 pixels width image:

![Gradient 1](https://github.com/jvinel/KinectSandbox/blob/master/snapshots/gradient1.png)
![Gradient 3](https://github.com/jvinel/KinectSandbox/blob/master/snapshots/gradient3.png)
