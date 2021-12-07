import { expect } from 'https://jspm.dev/@esm-bundle/chai';
import { add } from "/index.js";
// write your tests inline
describe('HTML tests', () => {
    it('works', () => {
        expect('foo').to.equal('foo');
    });
});

describe("Sum Tests", () => {
    it('should add numbers', () => {
        const result = add(1, 2);
        expect(result).to.equal(3);
    });

    it('should throw for non-numbers', () => {
        function throwFn() {
            add("1", 2);
        }
        expect(throwFn).to.throw(TypeError);
    });
});
