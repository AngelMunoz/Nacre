import "../../index.js"
import { expect } from 'https://jspm.dev/@esm-bundle/chai';
import { fixture } from 'https://jspm.dev/@open-wc/testing-helpers';

describe("Component Tests", () => {
    it('<my-element></my-element> renders', async () => {
        /**
         * @type {import('../../index.js').MyElement}
         */
        const element = await fixture("<my-element></my-element>")

        expect(element).to.not.be.undefined;
        expect(element).to.not.be.null;
    });
    it('Inner Div exists', async () => {
        /**
         * @type {import('../../index.js').MyElement}
         */
        const element = await fixture("<my-element></my-element>")
        let innerDiv = element.shadowRoot.querySelector("div")
        expect(innerDiv.getAttribute("data-value")).to.equal("10");
    });
});
