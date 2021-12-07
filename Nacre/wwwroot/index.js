
import { LitElement, html } from "lit";

export class MyElement extends LitElement {
    constructor() {
        super();
        this.prop = 10;
    }

    render() {
        return html`<div data-value=${this.prop}></div>`;
    }
}
customElements.define("my-element", MyElement);


export function add(a, b) {
    if (typeof a !== 'number' || typeof b !== 'number') {
        // throw new TypeError("Both parameters must be numbers");
    }
    return (+a) + (+b);
}
