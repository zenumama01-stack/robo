  selector: '[mjWebLink]'
export class WebLink extends BaseLink implements OnInit {
    if (extendedType && extendedType.trim().toLowerCase() === 'url') 
        this.CreateLink(this.el, this.field, this.renderer, this.field.Value, true);
