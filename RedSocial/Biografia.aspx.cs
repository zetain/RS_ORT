﻿using RedSocialBusiness;
using RedSocialDataSQLServer;
using RedSocialEntity;
using RedSocialWebUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Biografia : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            List<PublicacionEntity> listPublicaciones = null;

            if (SessionHelper.Grupo != null)
            {
                listPublicaciones = PublicacionBO.Listar(SessionHelper.Grupo);
            }

            if (SessionHelper.UsuarioAutenticado != null)
            {
                listPublicaciones = PublicacionBO.Listar(SessionHelper.UsuarioAutenticado);
            }

            rptPublicaciones.DataSource = listPublicaciones;
            rptPublicaciones.DataBind();

            rptGrupos.DataSource = GrupoBO.Listar(SessionHelper.UsuarioAutenticado, true);
            rptGrupos.DataBind();

            rptSolicitudes.DataSource = SolicitudBO.Listar(SessionHelper.UsuarioAutenticado);
            rptSolicitudes.DataBind();

            rptContactos.DataSource = UsuarioBO.BuscarUsuariosAmigos(SessionHelper.UsuarioAutenticado);
            rptContactos.DataBind();

        }
    }


    protected void rptPublicaciones_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            PublicacionEntity publicacion = (PublicacionEntity)e.Item.DataItem;

            HiddenField lblPublicacionId = (HiddenField)e.Item.FindControl("lblPublicacionId");
            Image imgUsuarioPost = (Image)e.Item.FindControl("imgPubUsuarioPost");
            Label lblNombreUsuario = (Label)e.Item.FindControl("lblPubNombreUsuario");
            Label lblPubMensaje = (Label)e.Item.FindControl("lblPubMensaje");
            Image imgPubImagen = (Image)e.Item.FindControl("imgPubImagen");
            Label lblPubFecha = (Label)e.Item.FindControl("lblPubFecha");
            Label imgPubRanking = (Label)e.Item.FindControl("imgPubRanking");

            Repeater rptComentarios = (Repeater)e.Item.FindControl("rptComentarios");

            lblPublicacionId.Value = publicacion.id.ToString();
            lblNombreUsuario.Text = publicacion.nombreUsuario;
            lblPubMensaje.Text = publicacion.descripcion;
            if (publicacion.imagen != null)
            {
                imgPubImagen.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(publicacion.imagen, 0, publicacion.imagen.Length); ;
            }
            lblPubFecha.Text = publicacion.actualizacion.ToString();

            imgPubRanking.Text = publicacion.calificacion.ToString();


            rptComentarios.DataSource = publicacion.listaComentarios;
            rptComentarios.DataBind();

        }
    }

    protected void rptComentarios_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            ComentarioEntity comentario = (ComentarioEntity)e.Item.DataItem;

            Label lblComNombre = (Label)e.Item.FindControl("lblComNombre");
            Label lblComTexto = (Label)e.Item.FindControl("lblComTexto");
            Label lblComFecha = (Label)e.Item.FindControl("lblComFecha");
            Label lblComPuntos = (Label)e.Item.FindControl("lblComPuntos");

            lblComNombre.Text = comentario.nombreUsuario;
            lblComTexto.Text = comentario.texto;
            lblComFecha.Text = comentario.fechaActualizacion.ToString();
            lblComPuntos.Text = comentario.calificacion.ToString();
        }
    }

    protected void btnPublicar_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtPublicar.Text))
        {
            PublicacionEntity publicacion = new PublicacionEntity();
            publicacion.usuarioID = SessionHelper.UsuarioAutenticado.id;
            publicacion.descripcion = txtPublicar.Text;
            publicacion.actualizacion = DateTime.Now;

            new PublicacionBO().Registrar(publicacion);

            Response.Redirect(Request.RawUrl);
        }
    }

    protected void btnBuscarUsuario_Click(object sender, EventArgs e)
    {
        Response.Redirect("BuscarUsuarios.aspx?id=" + txtBuscarUsuario.Text);
    }

    protected void rptPublicaciones_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "Comentar")
        {
            string textoComentario = ((TextBox)e.Item.FindControl("txtComentar")).Text;
            
            if (!string.IsNullOrWhiteSpace(textoComentario))
            {
                int calificacion = Convert.ToInt32(((DropDownList)e.Item.FindControl("ddlCalificacion")).SelectedValue);

                int publicacionId = Convert.ToInt32( ((HiddenField)e.Item.FindControl("lblPublicacionId")).Value);

                ComentarioEntity comentario = new ComentarioEntity();
                comentario.calificacion = calificacion;
                comentario.texto = textoComentario;

                comentario.usuarioID = SessionHelper.UsuarioAutenticado.id;
                comentario.publicacionID = publicacionId;

                new ComentarioBO().Registrar(comentario);

                Response.Redirect(Request.RawUrl);
            }
        }
    }


    protected void rptSolicitudes_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        SolicitudEntity solicitud = new SolicitudEntity();
        solicitud.id = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "Aceptar")
        {
            new SolicitudBO().Aceptar(solicitud);
        }
        else if (e.CommandName == "Rechazar")
        {
            new SolicitudBO().Rechazar(solicitud);
        }

        Response.Redirect(Request.RawUrl);
    }

    protected void rptGrupos_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "VerGrupo")
        {
            List<GrupoEntity> lista = GrupoBO.Listar(SessionHelper.UsuarioAutenticado, true);
            foreach (GrupoEntity grp in lista)
            {
                if (grp.id == Convert.ToInt32(e.CommandArgument))
                {
                    SessionHelper.AlmacenarGrupo(grp);
                    break;
                }            
            }
        }
    }
}