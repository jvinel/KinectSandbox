# KinectSandbox

KinectSandbox is a *.Net* application using a Kinect 1st generation device to create an augmented reality map representing elevation.

Image generated can then be displayed with a video projector on the ground.

This application was created after looking at this video: [İnteraktif Topoğrafya Haritas] (https://www.youtube.com/watch?v=PHz4LkeIeC0) and inspired by related software: [SARndbox] (http://idav.ucdavis.edu/~okreylos/ResDev/SARndbox/)

User interface provided give access to all settings required to calibrate sensor and produce a good quality image output.



All level detections and colorations are performed using an image processing framework [AForge.net] (http://www.aforgenet.com/), and therefore doesn't required any specific graphic card.