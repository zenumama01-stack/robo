export class PathData {
    public ID: number;
    public URL: string;
    public ParentPathData: PathData | null;
    public ChildPathData: PathData | null;
    constructor(id: number, name: string, url: string){
        this.ID = id;
        this.URL = url;
        this.ParentPathData = null;
        this.ChildPathData = null;
