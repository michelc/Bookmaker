using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Bookmaker.Helpers
{
    public static class HtmlHelperExtension
    {
        public static MvcHtmlString CaptionFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            string labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (String.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            var tag = new TagBuilder("label");
            tag.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
            tag.SetInnerText(labelText);

            if (metadata.ContainerType != null)
            {
                bool isRequired = metadata.ContainerType.GetProperty(metadata.PropertyName)
                                          .GetCustomAttributes(typeof(RequiredAttribute), false)
                                          .Length == 1;
                if (isRequired)
                {
                    tag.AddCssClass("is_required");
                    tag.InnerHtml += "<span>*</span>";
                }
            }

            return new MvcHtmlString(tag.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString ActionCancel(this HtmlHelper helper)
        {
            MvcHtmlString html = null;

            var id = helper.ViewContext.RouteData.Values["id"];
            if (id != null)
            {
                html = helper.ActionLink("Annuler", "Details", new { id = id.ToString() });
            }
            else
            {
                html = helper.ActionLink("Annuler", "Index");
            }

            return html;
        }

        public static MvcHtmlString ActionCrud(this HtmlHelper helper, string title)
        {
            var current_action = helper.ViewContext.RouteData.Values["action"].ToString().ToLower();

            var html = "";

            // Si on n'est pas sur la page Index
            if (current_action != "index")
            {
                // Alors, il faut un lien vers la page Index
                html += helper.ActionLink(title, "Index").ToString();
            }
            else
            {
                // Sinon, il n'y a pas besoin d'un lien vers la page Index
                html += title;
            }

            // Si on n'est pas sur la page Create
            html += " / ";
            if (current_action != "create")
            {
                // Alors, il faut un lien vers la page Create
                var current_controller = helper.ViewContext.RouteData.Values["controller"].ToString().ToLower();
                html += helper.ActionLink("Créer", "Create").ToString();
            }
            else
            {
                // Sinon, il n'y a pas besoin d'un lien vers la page Create
                html += "Créer";
            }

            // Si on a un identifiant de fiche
            var id = helper.ViewContext.RouteData.Values["id"];
            if (id != null)
            {
                // Alors, il faut générer les autres liens CRUD

                // Si on n'est pas sur la page Details
                html += " / ";
                if (current_action != "details")
                {
                    // Alors, il faut un lien vers la page Details
                    html += helper.ActionLink("Afficher", "Details", new { id = id.ToString() }).ToString();
                }
                else
                {
                    // Sinon, il n'y a pas besoin d'un lien vers la page Details
                    html += "Afficher";
                }

                // Si on n'est pas sur la page Edit
                html += " / ";
                if (current_action != "edit")
                {
                    // Alors, il faut un lien vers la page Edit
                    html += helper.ActionLink("Modifier", "Edit", new { id = id.ToString() }).ToString();
                }
                else
                {
                    // Sinon, il n'y a pas besoin d'un lien vers la page Edit
                    html += "Modifier";
                }

                // Si on n'est pas sur la page Delete
                html += " / ";
                if (current_action != "delete")
                {
                    // Alors, il faut un lien vers la page Delete
                    html += helper.ActionLink("Supprimer", "Delete", new { id = id.ToString() }).ToString();
                }
                else
                {
                    // Sinon, il n'y a pas besoin d'un lien vers la page Delete
                    html += "Supprimer";
                }

            }

            return new MvcHtmlString(html);
        }

        public static MvcHtmlString ActionCrudCopy(this HtmlHelper helper, string title)
        {
            var html = ActionCrud(helper, title).ToString();

            var current_action = helper.ViewContext.RouteData.Values["action"].ToString().ToLower();
            var id = helper.ViewContext.RouteData.Values["id"];

            // Si on a un identifiant de fiche
            if (id != null)
            {
                // Alors, il faut générer le lien Copy

                // Si on n'est pas sur la page Copy
                html += " / ";
                if (current_action != "copy")
                {
                    // Alors, il faut un lien vers la page Copy
                    html += helper.ActionLink("Copier", "Copy", new { id = id.ToString() }).ToString();
                }
                else
                {
                    // Sinon, il n'y a pas besoin d'un lien vers la page Copy
                    html += "Copier";
                }
            }

            return new MvcHtmlString(html);
        }

        public static MvcHtmlString ActionChildcrud(this HtmlHelper helper, string title, string child_controller, int parent_id = -1)
        {
            var current_controller = helper.ViewContext.RouteData.Values["controller"].ToString().ToLower();
            var current_action = helper.ViewContext.RouteData.Values["action"].ToString().ToLower();

            var parent_controller = "travels";

            var html = "";

            // Si on n'est pas sur la page Details du parent
            if ((current_action != "details") || (current_controller != parent_controller))
            {
                // Alors, il faut un lien vers la page Details du parent
                html += helper.ActionLink(title, "Details", parent_controller, new { id = parent_id.ToString() }, null).ToString();
            }
            else
            {
                // Sinon, il n'y a pas besoin d'un lien vers la page Details du parent
                html += title;
            }

            // Si on n'est pas sur la page Create
            html += " / ";
            if (current_action != "create")
            {
                // Alors, il faut un lien vers la page Create
                html += helper.ActionLink("Créer", "Create", child_controller, new { Parent_ID = parent_id.ToString() }, null).ToString();
            }
            else
            {
                // Sinon, il n'y a pas besoin d'un lien vers la page Create
                html += "Créer";
            }

            // Si on a un identifiant de fiche
            var id = helper.ViewContext.RouteData.Values["id"];
            if (current_action == "details")
            {
                if (current_controller != child_controller.ToLower())
                {
                    id = null;
                }
            }
            if (id != null)
            {
                // Alors, il faut générer les autres liens CRUD

                // Si on n'est pas sur la page Details
                html += " / ";
                if (current_action != "details")
                {
                    // Alors, il faut un lien vers la page Details
                    html += helper.ActionLink("Afficher", "Details", new { id = id.ToString() }).ToString();
                }
                else
                {
                    // Sinon, il n'y a pas besoin d'un lien vers la page Details
                    html += "Afficher";
                }

                // Si on n'est pas sur la page Edit
                html += " / ";
                if (current_action != "edit")
                {
                    // Alors, il faut un lien vers la page Edit
                    html += helper.ActionLink("Modifier", "Edit", new { id = id.ToString() }).ToString();
                }
                else
                {
                    // Sinon, il n'y a pas besoin d'un lien vers la page Edit
                    html += "Modifier";
                }

                // Si on n'est pas sur la page Delete
                html += " / ";
                if (current_action != "delete")
                {
                    // Alors, il faut un lien vers la page Delete
                    html += helper.ActionLink("Supprimer", "Delete", new { id = id.ToString() }).ToString();
                }
                else
                {
                    // Sinon, il n'y a pas besoin d'un lien vers la page Delete
                    html += "Supprimer";
                }

            }

            return new MvcHtmlString(html);
        }
    }
}