using System.Collections.Generic;
using UnityEngine;

    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ParticlePlexus : MonoBehaviour
    {
        public float maxDistance = 1f;
        public int maxConnections = 5;
        public int maxLineRenderers = 100;
        public float triangleTolerance = 1f;
        public float toleranceThreshold = 0.1f;

        public bool useTriangles = true;

        public Camera camera;
        private Vector3 _cameraTransform;
        public float cameraThreshold = 1f;

        public LineRenderer lineRendererTemplate;
        List<LineRenderer> _lineRenderers = new List<LineRenderer>();

        private Transform _transform;

        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particles;

        private Mesh _mesh;
        private Vector3[] _vertices;
        private int[] _triangleIDs;
        private Color[] _alphaIntencities;

        private int _maxParticles;

        private void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();

            _transform = transform;
            
            GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
            _mesh.name = "Plexus planes";
            
            _maxParticles = _particleSystem.main.maxParticles;

            if (camera != null)
                _cameraTransform = camera.transform.position;
        }

        private void LateUpdate()
        {
            _mesh.Clear();
            
            _vertices = new Vector3[_maxParticles*3];
            _triangleIDs = new int[_maxParticles*3];
            _alphaIntencities = new Color[_maxParticles*3];
            
            int lrIndex = 0;
            int lineRendererCount = _lineRenderers.Count;
            _particles = new ParticleSystem.Particle[_maxParticles];
            int aliveParticles = _particleSystem.GetParticles( _particles );
            
            int trianglePointsCount = 0;

            if (lineRendererCount > maxLineRenderers)
            {
                for (int i = maxLineRenderers; i < lineRendererCount; i++)
                {
                    Destroy(_lineRenderers[i].gameObject);
                }

                int removedCount = lineRendererCount - maxLineRenderers;
                _lineRenderers.RemoveRange(maxLineRenderers, removedCount);
                lineRendererCount -= removedCount;
            }

            if (maxConnections > 0 && maxLineRenderers > 0)
            {
                float maxDistanceSqr = maxDistance * maxDistance;
                
                _particleSystem.GetParticles(_particles);
                int particleCount = _particleSystem.particleCount;

                for (int i = 0; i < particleCount; i++)
                {
                    if (lrIndex == maxLineRenderers)
                    {
                        break;
                    }
                    
                    _cameraTransform = camera.transform.position;
                    
                    Vector3 p1_position = _particles[i].position;
                    
                    if (Vector3.SqrMagnitude(_cameraTransform - p1_position) < cameraThreshold * cameraThreshold)
                    {
                        _particles[i].remainingLifetime = 0f;
                        continue;
                    }

                    int connections = 0;
                    
                    for (int j = i + 1; j < particleCount; j++)
                    {
                        Vector3 p2_position = _particles[j].position;
                        float distanceSqr = Vector3.SqrMagnitude(p1_position - p2_position);

                        if (distanceSqr <= maxDistanceSqr + triangleTolerance)
                        {
                            LineRenderer lr;
                            if (lrIndex == lineRendererCount)
                            {
                                lr = Instantiate(lineRendererTemplate, _transform, false);
                                _lineRenderers.Add(lr);

                                lineRendererCount++;
                            }

                            lr = _lineRenderers[lrIndex];
                        
                            lr.enabled = true;
                        
                            lr.SetPosition(0,p1_position);
                            lr.SetPosition(1,p2_position);

                            lrIndex++;
                            connections++;

                            if (useTriangles)
                            {
                                for (int k = j + 1; k < particleCount; k++) //ищем третью точку
                                {
                                    Vector3 p3_position = _particles[k].position;
                                    float distance31Sqr = Vector3.SqrMagnitude(p1_position - p3_position);
                                    float distance32Sqr = Vector3.SqrMagnitude(p2_position - p3_position);

                                    if (distance31Sqr <= maxDistanceSqr+triangleTolerance && 
                                        distance32Sqr <= maxDistanceSqr+triangleTolerance)
                                    {
                                        float fadeFactor = 1f;
                                        float vertexAlpha = 1f;
                                        
                                        if (distance31Sqr > maxDistanceSqr)
                                        {
                                            fadeFactor = Mathf.Abs(maxDistanceSqr + triangleTolerance - distance31Sqr);
                                            if (fadeFactor > toleranceThreshold)
                                            {
                                                vertexAlpha = Remap(fadeFactor, 0f, triangleTolerance, 0f, 1f);
                                            }
                                            else
                                            {
                                                vertexAlpha = 0f;
                                            }
                                        }
                                        else if (distance32Sqr > maxDistanceSqr)
                                        {
                                            fadeFactor = Mathf.Abs(maxDistanceSqr + triangleTolerance - distance32Sqr);
                                            if (fadeFactor > toleranceThreshold)
                                            {
                                                vertexAlpha = Remap(fadeFactor, 0f, triangleTolerance, 0f, 1f);
                                            }
                                            else
                                            {
                                                vertexAlpha = 0f;
                                            }
                                        }

                                        Color aplhaValue = new Color(1,1,1,vertexAlpha);
                                        
                                        AddTriangleCoordinates(p1_position, p2_position, p3_position, aplhaValue, 
                                            trianglePointsCount);

                                        trianglePointsCount += 3;
                                        break;
                                    }
                                }
                                SetTriangles();
                            }

                            if (connections == maxConnections || lrIndex == maxLineRenderers)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            _particleSystem.SetParticles(_particles, aliveParticles);
        }
        
        void AddTriangleCoordinates(Vector3 p1, Vector3 p2, Vector3 p3, Color aplha, int triang)
        {
            _vertices[triang] = p1;
            _vertices[triang+1] = p2;
            _vertices[triang+2] = p3;
            
            _triangleIDs[triang] = triang;
            _triangleIDs[triang+1] = triang + 1;
            _triangleIDs[triang+2] = triang + 2;
            
            _alphaIntencities[triang] = aplha;
            _alphaIntencities[triang+1] = aplha;
            _alphaIntencities[triang+2] = aplha;
        }

        void SetTriangles()
        {
            
            _mesh.vertices = _vertices;
            _mesh.triangles = _triangleIDs;
            _mesh.colors = _alphaIntencities;
        }
        
        private float Remap(float input, float inputMin, float inputMax, float min, float max)
        {
            return min + (input - inputMin) * (max - min) / (inputMax - inputMin);
        }
    }