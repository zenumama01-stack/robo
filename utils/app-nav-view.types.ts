export class StubData {
    public Children: StubData[];
    public Expanded: boolean = false;
    constructor(name: string, children: StubData[]) {
        this.Children = children;
