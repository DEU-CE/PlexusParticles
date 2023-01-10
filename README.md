# PlexusParticles
Plexus effect for Particle System. The core mechanic of building plexus effect is comparison the square ditance between particle pairs with square of "Max Distance" parameter. If distance is less, then Unity builds line renderer between particle pair.
It also lets change count of maximal connections to one particle - use "Max Connections" parameter for it.

Available generation of triangle planes with plexus effect - use the checkbox "Use Triangles". It involves one more for-loop for searching suitable third particle and sending particle coordinates to Mesh component.
![Plexus_2](https://user-images.githubusercontent.com/94839324/211629868-61e71bf6-8326-446d-b3de-2c5605c1e878.png)
![Plexus_1](https://user-images.githubusercontent.com/94839324/211629875-bfca403e-e788-4a4d-b53f-dd2778b0cfc6.png)
