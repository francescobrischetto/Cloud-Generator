# Cloudy Sky Generator using 3D Perlin Noise

Artificial Intelligence for Videogames 2020/21

My project consists of an offline generator of cloudy sky in a configurable 3D space. It is possible to save the generated mesh into an asset to use it as baked results. It is also possible to produce the same result using a seed. Its purpose is helping the level designer to generate a natural looking sky clouds formation, that can also change using the threshold value to simulate atmospheric phenomena. Unfortunately, the approach used to generate clouds is not time-efficient and it takes more time as long as the requested area grows. For this reason, I marked this as an “offline” generator, even if it would be still possible to generate the sky in a small area and repeat the pattern generated in the whole game world, using this approach in an “online” fashion.



To see test Scene go in:  Cloud-Generator/Assets/Cloud Generator Project/CloudGeneratorScene.unity

