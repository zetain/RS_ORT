﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using RedSocialEntity;
using RedSocialData;
namespace RedSocialDataSQLServer
{
    public class PublicacionDA
    {
        public PublicacionDA()
        {

        }
        #region Métodos Privados
        private PublicacionEntity CrearPublicacion(SqlDataReader cursor)
        {
            PublicacionEntity publicacion = null;
            return publicacion;
        }
        #endregion Métodos Privados

        #region Métodos Públicos

        public bool Insertar(PublicacionEntity publicacion)
        {
            try
            {
                using (SqlConnection conexion = ConexionDA.ObtenerConexion())
                {
                    using (SqlCommand comando = new SqlCommand("PublicacionInsert", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        SqlCommandBuilder.DeriveParameters(comando);

                        comando.Parameters["@UsuarioID"].Value = publicacion.usuarioID;
                        comando.Parameters["@GrupoID"].Value = publicacion.grupoID;
                        comando.Parameters["@Descripcion"].Value = publicacion.descripcion.Trim();
                        comando.Parameters["@PublicacionActualizacion"].Value = publicacion.actualizacion;
                        comando.Parameters["@ComentarioCalificacion"].Value = publicacion.calificacion;
                        comando.Parameters["@PublicacionImagen"].Value = publicacion.imagen;
                        comando.ExecuteNonQuery();
                        publicacion.id = Convert.ToInt32(comando.Parameters["@RETURN_VALUE"].Value);
                    }

                    conexion.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new ExcepcionDA("Se produjo un error al insertar la publicacion.", ex);
            }
        }
        public static List<PublicacionEntity> BuscarPublicaciones(object filtro)
        {
            try
            {
                string query = "SELECT * FROM Publicacion p left join Mensaje m on m.PublicacionID = p.PublicacionID WHERE ";
                string parameterID = "";
                List<PublicacionEntity> listaPublicaciones = new List<PublicacionEntity>();
                if (filtro.GetType().Name == "GrupoEntity")
                {
                    parameterID = ((GrupoEntity)filtro).id.ToString();
                    query += "p.GrupoID = @Parameter_ID";
                }
                if (filtro.GetType().Name == "UsuarioEntity")
                {
                    parameterID = ((UsuarioEntity)filtro).id.ToString();
                    query += "p.UsuarioID = @Parameter_ID";
                }

                using (SqlConnection conexion = ConexionDA.ObtenerConexion())
                {
                    using (SqlCommand comando = new SqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@Parameter_ID", parameterID);
                        using (SqlDataReader cursor = comando.ExecuteReader())
                        {
                            int codant = 0;
                            PublicacionEntity publicacion = new PublicacionEntity();
                            publicacion.listaComentarios = new List<ComentarioEntity>();
                            while (cursor != null && cursor.Read())
                            {
                                int pubID = (int)cursor["PublicacionID"];
                                if (codant != pubID)
                                {
                                    if(codant!=0)
                                        listaPublicaciones.Add(publicacion);

                                    publicacion.id = pubID;
                                    if (cursor["GrupoID"] != DBNull.Value)
                                        publicacion.grupoID = (int)cursor["GrupoID"];
                                    publicacion.usuarioID = (int)cursor["UsuarioID"];
                                    publicacion.descripcion = (string)cursor["Descripcion"];
                                    publicacion.actualizacion = (DateTime)cursor["PublicacionActualizacion"];
                                    publicacion.calificacion = (int)cursor["PublicacionCalificacion"];
                                    if (cursor["PublicacionImagen"] != DBNull.Value)
                                        publicacion.imagen = (byte[])cursor["PublicacionImagen"];

                                    codant = pubID;
                                }

                                if (cursor["MensajeID"] != DBNull.Value)
                                {
                                    ComentarioEntity comentario = new ComentarioEntity();
                                    comentario.id = (int)cursor["MensajeID"];
                                    comentario.usuarioID = (int)cursor["UsuarioID"];
                                    comentario.texto = cursor["MensajeTexto"].ToString();
                                    comentario.fechaActualizacion = (DateTime)cursor["MensajeFecha"];

                                    publicacion.listaComentarios.Add(comentario);
                                }
                                    
                            }
                            listaPublicaciones.Add(publicacion);

                            cursor.Close();
                        }
                    }

                    conexion.Close();
                }

                return listaPublicaciones;
            }
            catch (Exception ex)
            {
                throw new ExcepcionDA("Se produjo un error al buscar la lista de publicaciones.", ex);
            }
        }

        public void Actualizar(int id, byte[] archivoFoto)
        {
            try
            {
                //FileInfo infoArchivo = new FileInfo(nombreArchivo);

                //string rutaFotos = ConfigurationManager.AppSettings["RutaFotos"];
                //string nuevoNombreArchivo = id.ToString() + infoArchivo.Extension;

                //using (FileStream archivo = File.Create(rutaFotos + nuevoNombreArchivo))
                //{
                //    archivo.Write(archivoFoto, 0, archivoFoto.Length);
                //    archivo.Close();
                //}

                using (SqlConnection conexion = ConexionDA.ObtenerConexion())
                {
                    using (SqlCommand comando = new SqlCommand("PublicacionActualizarFoto", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        SqlCommandBuilder.DeriveParameters(comando);

                        comando.Parameters["@PublicacionID"].Value = id;                        
                        comando.Parameters["@PublicacionImagen"].Value = archivoFoto;
                        comando.ExecuteNonQuery();
                    }

                    conexion.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ExcepcionDA("Se produjo un error al actualizar la imagen.", ex);
            }
        }
        #endregion Métodos Públicos


    }
}
