import { expect } from 'https://jspm.dev/@esm-bundle/chai';
import { add } from "/index.js";

describe("Component Tests", () => {
    before(() => {
        // runs once before the first test in this block
        let div = document.createElement("div");
        div.id = "findMe";
        document.body.appendChild(div);
    });
    it('div exists', () => {
        let div = document.querySelector("#findMe");
        expect(div).to.not.be.undefined;
        expect(div).to.not.be.null;
    });
});