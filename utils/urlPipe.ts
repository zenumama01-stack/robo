@Pipe({standalone: false, name: 'formatUrl'})
export class URLPipe implements PipeTransform {
    if (value && !value.includes('http')) 
        return 'https://' + value;
