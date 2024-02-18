using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CrmCorner.TagHelpers
{
    public class UserPictureThumbnailTagHelper : TagHelper
    {
        public string? PictureUrl { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "img";
            if (string.IsNullOrEmpty(PictureUrl))
            {
                output.Attributes.SetAttribute("src", "/userprofilepicture/defaultpp.png");
            }
            else
            {
                output.Attributes.SetAttribute("src", $"/userprofilepicture/{PictureUrl}");
            }


            base.Process(context, output);
        }
    }
}
